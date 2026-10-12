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

    public LoopbackTlsGameServer(RoomSessionCoordinator sessions, X509Certificate2 certificate,
        int maxConnections = 4)
    {
        _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        if (!certificate.HasPrivateKey)
            throw new ArgumentException("TLS certificate must carry a private key.", nameof(certificate));
        if (maxConnections is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(maxConnections));
        _slots = new SemaphoreSlim(maxConnections, maxConnections);
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
                _slots.Release();
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

                await Send(tls, "HELLO ARTHEA-TLS/0", cancellation);
                var initial = await Receive(tls, cancellation);
                if (initial is null) return;
                SessionHandle handle;
                if (initial.StartsWith("LOGIN ", StringComparison.Ordinal))
                {
                    var playerId = initial[6..];
                    await Send(tls, "PASSWORD", cancellation);
                    var passwordLine = await Receive(tls, cancellation);
                    if (passwordLine is null ||
                        !passwordLine.StartsWith("PASS ", StringComparison.Ordinal))
                        return;
                    handle = _sessions.Connect(playerId + "\n" + passwordLine[5..], "tavern");
                }
                else if (initial.StartsWith("RESUME ", StringComparison.Ordinal))
                    handle = _sessions.Resume(initial[7..]);
                else return;

                token = handle.Token;
                await Send(tls, "SESSION " + handle.Token, cancellation);
                await Send(tls, "ROOM " + _sessions.CurrentRoom(token), cancellation);
                while (!cancellation.IsCancellationRequested)
                {
                    var line = await Receive(tls, cancellation);
                    if (line is null) break;
                    if (line == "QUIT")
                    {
                        _sessions.Logout(token);
                        token = null;
                        await Send(tls, "BYE", cancellation);
                        break;
                    }
                    if (line == "LOOK")
                        await Send(tls, "ROOM " + _sessions.CurrentRoom(token), cancellation);
                    else if (line.StartsWith("SAY ", StringComparison.Ordinal))
                    {
                        var message = _sessions.Say(token, line[4..]);
                        await Send(tls, "SENT " + message.Sequence, cancellation);
                    }
                    else if (line.StartsWith("POLL ", StringComparison.Ordinal) &&
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
                            await Send(tls, "ROOM " + _sessions.CurrentRoom(token), cancellation);
                        else await Send(tls, "ERR no_exit", cancellation);
                    }
                    else if (line == "PING") await Send(tls, "PONG", cancellation);
                    else await Send(tls, "ERR command", cancellation);
                }
            }
            catch (Exception ex) when (ex is AuthenticationException or IOException or
                                           SocketException or OperationCanceledException or
                                           InvalidOperationException or ArgumentException or
                                           OverflowException)
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
        direction = text switch
        {
            "NORTH" => Direction.North, "SOUTH" => Direction.South,
            "EAST" => Direction.East, "WEST" => Direction.West,
            "UP" => Direction.Up, "DOWN" => Direction.Down,
            _ => (Direction)(-1)
        };
        return Enum.IsDefined(direction);
    }

    private static async Task<string?> Receive(SslStream stream, CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        using var bytes = new MemoryStream(capacity: 128);
        var one = new byte[1];
        while (bytes.Length <= 1024)
        {
            var count = await stream.ReadAsync(one, timeout.Token);
            if (count == 0) return null;
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
