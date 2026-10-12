using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Arthea.Core.Social;
using Arthea.Core.World;

namespace Arthea.Transport;

/// <summary>
/// A loopback-only encrypted text protocol proof. Not a Telnet/GMCP implementation,
/// an Internet server, or a production credential ingress. No public bind option.
/// </summary>
public sealed class LoopbackTlsGameServer : IAsyncDisposable
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly RoomSessionCoordinator _sessions;
    private readonly X509Certificate2 _certificate;
    private readonly CancellationTokenSource _stop = new();
    private readonly SemaphoreSlim _slots;
    private readonly List<Task> _pending = new();
    private readonly object _gate = new();
    private Task? _accept;
    private bool _started;
    private readonly bool _enableTelnetGmcp;
    private readonly TimeSpan _preAuthReadTimeout;
    private readonly TimeSpan _authenticatedIdleTimeout;
    private readonly TimeSpan _incompleteCommandTimeout;

    public LoopbackTlsGameServer(RoomSessionCoordinator sessions, X509Certificate2 certificate,
        int maxConnections = 4, bool enableTelnetGmcp = false,
        TimeSpan? preAuthReadTimeout = null, TimeSpan? authenticatedIdleTimeout = null,
        TimeSpan? incompleteCommandTimeout = null)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        if (!certificate.HasPrivateKey)
            throw new ArgumentException("TLS certificate must carry a private key.", nameof(certificate));
        if (maxConnections is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(maxConnections));
        _slots = new SemaphoreSlim(maxConnections, maxConnections);
        _enableTelnetGmcp = enableTelnetGmcp;
        _preAuthReadTimeout = ValidateDeadline(preAuthReadTimeout ?? TimeSpan.FromSeconds(15), nameof(preAuthReadTimeout));
        _authenticatedIdleTimeout = ValidateDeadline(authenticatedIdleTimeout ?? TimeSpan.FromMinutes(15), nameof(authenticatedIdleTimeout));
        _incompleteCommandTimeout = ValidateDeadline(incompleteCommandTimeout ?? TimeSpan.FromSeconds(15), nameof(incompleteCommandTimeout));
    }

    private static TimeSpan ValidateDeadline(TimeSpan deadline, string name)
    {
        if (deadline < TimeSpan.FromMilliseconds(100) || deadline > TimeSpan.FromHours(24))
            throw new ArgumentOutOfRangeException(name, "Deadline must be between 100 ms and 24 hours.");
        return deadline;
    }

    public IPEndPoint Start()
    {
        if (_started) throw new InvalidOperationException("Server already started.");
        _started = true;
        _listener.Start(16);
        _accept = AcceptAsync(_stop.Token);
        return (IPEndPoint)_listener.LocalEndpoint;
    }

    private async Task AcceptAsync(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(cancellation); }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { break; }
            catch (SocketException) when (cancellation.IsCancellationRequested) { break; }
            if (!_slots.Wait(0))
            {
                client.Dispose();
                continue;
            }
            var task = HandleAsync(client, cancellation);
            lock (_gate) _pending.Add(task);
            _ = task.ContinueWith(completed =>
            {
                _ = completed.Exception; // Observe faults without leaking secrets to logs.
                try { _slots.Release(); }
                catch (ObjectDisposedException) { }
                lock (_gate) _pending.Remove(completed);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }

    private async Task HandleAsync(TcpClient client, CancellationToken cancellation)
    {
        string? token = null;
        using (client)
        using (var tls = new SslStream(client.GetStream(), leaveInnerStreamOpen: false))
        {
            try
            {
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                deadline.CancelAfter(TimeSpan.FromSeconds(5));
                await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = _certificate,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                }, deadline.Token);

                var gmcp = _enableTelnetGmcp ? new TelnetGmcpCodec() : null;
                if (gmcp is not null)
                    await tls.WriteAsync(TelnetGmcpCodec.OfferGmcp(), cancellation);
                await Send(tls, "HELLO ARTHEA-TLS/0", cancellation);
                var initial = await Receive(tls, gmcp, null, cancellation);
                if (initial is null) return;
                SessionHandle handle;
                if (initial.StartsWith("LOGIN ", StringComparison.OrdinalIgnoreCase))
                {
                    var playerId = initial[6..];
                    await Send(tls, "PASSWORD", cancellation);
                    var passwordLine = await Receive(tls, gmcp, null, cancellation);
                    if (passwordLine is null ||
                        !passwordLine.StartsWith("PASS ", StringComparison.OrdinalIgnoreCase))
                        return;
                    handle = _sessions.Connect(playerId + "\n" + passwordLine[5..], "tavern");
                }
                else if (initial.StartsWith("RESUME ", StringComparison.OrdinalIgnoreCase))
                    handle = _sessions.Resume(initial[7..]);
                else return;

                token = handle.Token;
                await Send(tls, "SESSION " + handle.Token, cancellation);
                await SendRoom(tls, gmcp, token, cancellation);
                while (!cancellation.IsCancellationRequested)
                {
                    var line = await Receive(tls, gmcp, token, cancellation);
                    if (line is null) break;
                    if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        _sessions.Logout(token);
                        token = null;
                        await Send(tls, "BYE", cancellation);
                        break;
                    }
                    if (line.Equals("LOOK", StringComparison.OrdinalIgnoreCase))
                        await SendRoom(tls, gmcp, token, cancellation);
                    else if (line.StartsWith("SAY ", StringComparison.OrdinalIgnoreCase))
                    {
                        var message = _sessions.Say(token, line[4..]);
                        await Send(tls, "SENT " + message.Sequence, cancellation);
                    }
                    else if (line.StartsWith("POLL ", StringComparison.OrdinalIgnoreCase) &&
                             long.TryParse(line[5..], out var cursor) && cursor >= 0)
                    {
                        var replay = _sessions.Replay(token, cursor, maxCount: 32);
                        if (replay.HasGap) await Send(tls, "GAP", cancellation);
                        foreach (var message in replay.Messages)
                            await Send(tls, "MSG " + message.Sequence + " " +
                                message.SenderId + ": " + message.Text, cancellation);
                        await Send(tls, "END", cancellation);
                    }
                    else if (TryDirection(line, out var direction))
                    {
                        if (_sessions.TryMove(token, direction))
                            await SendRoom(tls, gmcp, token, cancellation);
                        else await Send(tls, "ERR no_exit", cancellation);
                    }
                    else if (line.Equals("PING", StringComparison.OrdinalIgnoreCase)) await Send(tls, "PONG", cancellation);
                    else await Send(tls, "ERR command", cancellation);
                }
            }
            catch (Exception ex) when (ex is AuthenticationException or IOException or
                                           SocketException or OperationCanceledException or
                                           InvalidOperationException or ArgumentException or
                                           OverflowException or InvalidDataException)
            {
                // Fail closed. No exception details, tokens, or credentials are logged.
            }
            finally
            {
                if (token is not null)
                {
                    try { _sessions.Suspend(token); }
                    catch (InvalidOperationException) { }
                }
            }
        }
    }

    private static bool TryDirection(string text, out Direction direction)
    {
        direction = text.ToUpperInvariant() switch
        {
            "NORTH" => Direction.North, "SOUTH" => Direction.South,
            "EAST" => Direction.East, "WEST" => Direction.West,
            "UP" => Direction.Up, "DOWN" => Direction.Down,
            _ => (Direction)(-1)
        };
        return Enum.IsDefined(direction);
    }

    private async Task SendRoom(SslStream stream, TelnetGmcpCodec? gmcp,
        string token, CancellationToken cancellation)
    {
        var room = _sessions.RoomInfo(token); // Identity derived from authenticated bearer.
        await Send(stream, "ROOM " + room.Id, cancellation);
        if (gmcp?.EncodeRoomInfo(room) is { } bytes)
            await stream.WriteAsync(bytes, cancellation);
    }

    private async Task<string?> Receive(SslStream stream, TelnetGmcpCodec? gmcp,
        string? token, CancellationToken cancellation)
    {
        if (gmcp is null)
            return await ReceivePlain(stream, token is null ? _preAuthReadTimeout : _authenticatedIdleTimeout, cancellation);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var idleDeadline = token is null ? _preAuthReadTimeout : _authenticatedIdleTimeout;
        timeout.CancelAfter(idleDeadline);
        var wasPartial = false;
        var single = new byte[1];
        while (true)
        {
            var count = await stream.ReadAsync(single, timeout.Token);
            if (count == 0) return null;
            var advertised = gmcp.SupportsRoomInfo;
            var batch = gmcp.Feed(single);
            // Count incomplete commands/subnegotiations against one fixed budget:
            // additional bytes cannot extend an in-progress frame indefinitely.
            var isPartial = gmcp.HasPendingInput;
            if (!wasPartial && isPartial) timeout.CancelAfter(_incompleteCommandTimeout);
            else if (wasPartial && !isPartial) timeout.CancelAfter(idleDeadline);
            wasPartial = isPartial;
            foreach (var response in batch.Replies)
                await stream.WriteAsync(response, timeout.Token);
            if (!advertised && gmcp.SupportsRoomInfo && token is not null)
            {
                var room = _sessions.RoomInfo(token);
                if (gmcp.EncodeRoomInfo(room) is { } frame)
                    await stream.WriteAsync(frame, timeout.Token);
            }
            if (batch.Lines.Count > 0) return batch.Lines[0];
        }
    }

    private async Task<string?> ReceivePlain(SslStream stream, TimeSpan idleDeadline,
        CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(idleDeadline);
        using var bytes = new MemoryStream(capacity: 128);
        var one = new byte[1];
        while (bytes.Length <= 1024)
        {
            var count = await stream.ReadAsync(one, timeout.Token);
            if (count == 0) return null;
            if (bytes.Length == 0 && one[0] != '\n')
                timeout.CancelAfter(_incompleteCommandTimeout);
            if (one[0] == '\n') break;
            if (one[0] == 0) throw new InvalidDataException("Invalid input.");
            bytes.WriteByte(one[0]);
        }
        if (bytes.Length > 1024) throw new InvalidDataException("Line limit exceeded.");
        var payload = bytes.ToArray();
        if (payload.Length > 0 && payload[^1] == '\r')
            payload = payload[..^1];
        var text = StrictUtf8.GetString(payload);
        if (text.Any(c => char.IsControl(c))) throw new InvalidDataException("Control character in input.");
        return text;
    }

    private static Task Send(SslStream stream, string text, CancellationToken cancellation) =>
        stream.WriteAsync(StrictUtf8.GetBytes(text + "\n"), cancellation).AsTask();

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _listener.Stop();
        if (_accept is not null)
        {
            try { await _accept; }
            catch (SocketException) { }
        }
        Task[] pending;
        lock (_gate) pending = _pending.ToArray();
        try { await Task.WhenAll(pending).WaitAsync(TimeSpan.FromSeconds(5)); }
        catch (TimeoutException) { }
        _slots.Dispose();
        _stop.Dispose();
    }
}
