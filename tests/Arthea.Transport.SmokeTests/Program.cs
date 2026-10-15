using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Arthea.Core.World;
using Arthea.Core.Social;
using Arthea.Transport;

static void Check(bool value, string label)
{
    if (!value) throw new Exception("TLS transport contract failed: " + label);
}
static void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name + ": " + label);
}
static async Task Send(SslStream ssl, string line)
{
    await ssl.WriteAsync(Encoding.UTF8.GetBytes(line + "\n"));
}
static async Task<string> Read(SslStream ssl)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var bytes = new List<byte>();
    var buffer = new byte[1];
    while (true)
    {
        var read = await ssl.ReadAsync(buffer, cts.Token);
        if (read == 0) throw new Exception("Unexpected connection closure.");
        if (buffer[0] == '\n') break;
        bytes.Add(buffer[0]);
    }
    return Encoding.UTF8.GetString(bytes.ToArray());
}
static async Task<(TcpClient Socket, SslStream Ssl)> Connect(IPEndPoint endpoint,
    X509Certificate2 expected)
{
    var socket = new TcpClient();
    await socket.ConnectAsync(endpoint.Address, endpoint.Port);
    var ssl = new SslStream(socket.GetStream(), false, (_, cert, _, _) =>
        cert?.GetCertHashString() == expected.GetCertHashString());
    await ssl.AuthenticateAsClientAsync("localhost");
    Check(await Read(ssl) == "HELLO ARTHEA-TLS/0", "secure protocol preface");
    return (socket, ssl);
}
static async Task<string> Login(SslStream ssl, string id, string password)
{
    await Send(ssl, "LOGIN " + id);
    Check(await Read(ssl) == "PASSWORD", "password requested only after TLS");
    await Send(ssl, "PASS " + password);
    var session = await Read(ssl);
    Check(session.StartsWith("SESSION ", StringComparison.Ordinal), "session after verified login");
    Check(await Read(ssl) == "ROOM tavern", "initial room is correct");
    return session[8..];
}

using var key = RSA.Create(2048);
var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);
request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
var san = new SubjectAlternativeNameBuilder();
san.AddDnsName("localhost");
request.CertificateExtensions.Add(san.Build());
using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1),
    DateTimeOffset.UtcNow.AddHours(1));
var rooms = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "Tavern",
        new Dictionary<Direction,string> { [Direction.North] = "square" }),
    new RoomDefinition("square", "Square",
        new Dictionary<Direction,string> { [Direction.South] = "tavern" })
});
var auth = new PasswordAccountAuthenticator(new[]
{
    PasswordAccountAuthenticator.Provision("alice", "sample-alice-only-123!"),
    PasswordAccountAuthenticator.Provision("bob", "sample-bob-only-123!")
});
var sessions = new RoomSessionCoordinator(new RoomChatRuntime(rooms), auth);
// All read/write deadline inputs must reject an unbounded or unreasonable value.
Reject<ArgumentOutOfRangeException>(
    () => new LoopbackTlsGameServer(sessions, certificate, writeTimeout: TimeSpan.Zero),
    "zero outbound deadline rejected");
Reject<ArgumentOutOfRangeException>(
    () => new LoopbackTlsGameServer(sessions, certificate, writeTimeout: TimeSpan.FromHours(25)),
    "unbounded outbound deadline rejected");
await using var server = new LoopbackTlsGameServer(sessions, certificate);
var endpoint = server.Start();
Check(IPAddress.IsLoopback(endpoint.Address), "server only binds to loopback");
using (var untrusted = new TcpClient())
{
    await untrusted.ConnectAsync(endpoint.Address, endpoint.Port);
    using var invalid = new SslStream(untrusted.GetStream(), false);
    try
    {
        await invalid.AuthenticateAsClientAsync("localhost");
        throw new Exception("Untrusted self-signed server certificate was accepted.");
    }
    catch (AuthenticationException) { }
}
using (var plain = new TcpClient())
{
    await plain.ConnectAsync(endpoint.Address, endpoint.Port);
    await plain.GetStream().WriteAsync(Encoding.UTF8.GetBytes("LOGIN alice\nPASS sample-alice-only-123!\n"));
    // Plaintext cannot trigger game login; subsequent authenticated login proves
    // no account slot or session was created by an unencrypted credential attempt.
}

var denied = await Connect(endpoint, certificate);
await Send(denied.Ssl, "LOGIN alice");
Check(await Read(denied.Ssl) == "PASSWORD", "TLS-only credential prompt");
await Send(denied.Ssl, "PASS wrong-synthetic-password");
var deniedClosed = false;
try { await Read(denied.Ssl); }
catch (Exception e) when (e is IOException ||
    e.Message == "Unexpected connection closure.") { deniedClosed = true; }
Check(deniedClosed, "incorrect password cannot create a session");
denied.Ssl.Dispose();
denied.Socket.Dispose();

var oversized = await Connect(endpoint, certificate);
await Send(oversized.Ssl, "LOGIN " + new string('x', 1100));
var oversizedClosed = false;
try { await Read(oversized.Ssl); }
catch (Exception e) when (e is IOException ||
    e.Message == "Unexpected connection closure.") { oversizedClosed = true; }
Check(oversizedClosed, "oversized TLS command is rejected");
oversized.Ssl.Dispose();
oversized.Socket.Dispose();

var a = await Connect(endpoint, certificate);
var b = await Connect(endpoint, certificate);
var aliceToken = await Login(a.Ssl, "alice", "sample-alice-only-123!");
var bobToken = await Login(b.Ssl, "bob", "sample-bob-only-123!");
Check(aliceToken != bobToken, "independent identities");
await Send(a.Ssl, "SAY Hello Bob");
Check(await Read(a.Ssl) == "SENT 1", "room chat accepted");
await Send(b.Ssl, "POLL 0");
Check(await Read(b.Ssl) == "MSG 1 alice: Hello Bob", "authorized room replay");
Check(await Read(b.Ssl) == "END", "bounded replay terminator");
await Send(b.Ssl, "NORTH");
Check(await Read(b.Ssl) == "ROOM square", "move to adjacent room");
await Send(b.Ssl, "POLL 0");
Check(await Read(b.Ssl) == "END", "other room cannot replay tavern chat");
await Send(b.Ssl, "SOUTH");
Check(await Read(b.Ssl) == "ROOM tavern", "return to tavern");
await Send(a.Ssl, "SAY Reconnect test");
Check(await Read(a.Ssl) == "SENT 2", "new room event");
await Send(b.Ssl, "QUIT");
Check(await Read(b.Ssl) == "BYE", "explicit logout");
b.Ssl.Dispose();
b.Socket.Dispose();
a.Ssl.Dispose();
a.Socket.Dispose();
// The server suspends Alice upon socket disconnect; resume has to consume the
// old bearer and rotate it, without requiring old database credentials.
await Task.Delay(200);
var a2 = await Connect(endpoint, certificate);
await Send(a2.Ssl, "RESUME " + aliceToken);
var rotated = await Read(a2.Ssl);
Check(rotated.StartsWith("SESSION ", StringComparison.Ordinal) &&
    rotated[8..] != aliceToken, "resume rotates bearer");
Check(await Read(a2.Ssl) == "ROOM tavern", "room continuity on reconnect");
await Send(a2.Ssl, "POLL 1");
Check(await Read(a2.Ssl) == "MSG 2 alice: Reconnect test", "bounded replay after reconnect");
Check(await Read(a2.Ssl) == "END", "replay terminator after reconnect");
await Send(a2.Ssl, "QUIT");
Check(await Read(a2.Ssl) == "BYE", "logout succeeds");
a2.Ssl.Dispose();
a2.Socket.Dispose();

// A second isolated listener tests the opt-in RFC-854 Telnet/GMCP negotiation
// over TLS without changing existing plain line-protocol behavior.
var telnetSessions = new RoomSessionCoordinator(new RoomChatRuntime(rooms), auth);
await using var telnet = new LoopbackTlsGameServer(telnetSessions, certificate,
    enableTelnetGmcp: true);
var telnetEndpoint = telnet.Start();
static async Task<(TcpClient Socket, SslStream Stream)> ConnectTelnet(
    IPEndPoint endpoint, X509Certificate2 expected)
{
    var socket = new TcpClient();
    await socket.ConnectAsync(endpoint.Address, endpoint.Port);
    var ssl = new SslStream(socket.GetStream(), false, (_, cert, _, _) =>
        cert?.GetCertHashString() == expected.GetCertHashString());
    await ssl.AuthenticateAsClientAsync("localhost");
    var prefix = new byte[3];
    await ssl.ReadExactlyAsync(prefix);
    Check(prefix.SequenceEqual(new byte[] { 255, 251, 201 }), "Telnet WILL GMCP offered");
    Check(await Read(ssl) == "HELLO ARTHEA-TLS/0", "telnet greeting");
    return (socket, ssl);
}
static async Task<string> ReadGmcp(SslStream ssl)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var prefix = new byte[3];
    await ssl.ReadExactlyAsync(prefix, timeout.Token);
    Check(prefix.SequenceEqual(new byte[] { 255, 250, 201 }), "GMCP IAC SB header");
    using var data = new MemoryStream();
    var one = new byte[1];
    while (data.Length < 4096)
    {
        await ssl.ReadExactlyAsync(one, timeout.Token);
        if (one[0] == 255)
        {
            await ssl.ReadExactlyAsync(one, timeout.Token);
            if (one[0] == 240) return Encoding.UTF8.GetString(data.ToArray());
            if (one[0] != 255) throw new Exception("Invalid GMCP escaped byte");
        }
        data.WriteByte(one[0]);
    }
    throw new Exception("Oversized GMCP test response");
}
static byte[] GmcpSub(string value) =>
    [255, 250, 201, ..Encoding.UTF8.GetBytes(value), 255, 240];

// Case-insensitive command verbs: the user's message text and secret bytes are not case folded.
// Telnet option frames must not leave a partial-command timer active after
// negotiation completes; subsequent legitimate human idle time is permitted.
var timedGmcpSessions = new RoomSessionCoordinator(new RoomChatRuntime(rooms), auth);
await using var timedGmcpServer = new LoopbackTlsGameServer(timedGmcpSessions, certificate,
    enableTelnetGmcp: true, preAuthReadTimeout: TimeSpan.FromSeconds(3),
    authenticatedIdleTimeout: TimeSpan.FromSeconds(3),
    incompleteCommandTimeout: TimeSpan.FromMilliseconds(150));
var timedGmcp = await ConnectTelnet(timedGmcpServer.Start(), certificate);
await timedGmcp.Stream.WriteAsync(new byte[] { 255, 253, 201 });
await Login(timedGmcp.Stream, "alice", "sample-alice-only-123!");
await Task.Delay(450);
await Send(timedGmcp.Stream, "PING");
Check(await Read(timedGmcp.Stream) == "PONG",
    "completed Telnet negotiation leaves authenticated client idle");
await Send(timedGmcp.Stream, "QUIT");
Check(await Read(timedGmcp.Stream) == "BYE", "Telnet idle test logs out");
timedGmcp.Stream.Dispose();
timedGmcp.Socket.Dispose();

var structured = await ConnectTelnet(telnetEndpoint, certificate);
await structured.Stream.WriteAsync(new byte[] { 255, 253, 201 });
await structured.Stream.WriteAsync(GmcpSub("Core.Hello {\"client\":\"ArtheaTest\",\"version\":\"1\"}"));
await structured.Stream.WriteAsync(GmcpSub("Core.Supports.Set [\"Room 1\"]"));
await Send(structured.Stream, "login alice");
Check(await Read(structured.Stream) == "PASSWORD", "negotiation precedes encrypted login");
await Send(structured.Stream, "pass sample-alice-only-123!");
Check((await Read(structured.Stream)).StartsWith("SESSION ", StringComparison.Ordinal),
    "authenticated token issued under TLS/Telnet");
Check(await Read(structured.Stream) == "ROOM tavern", "text room delivered with GMCP");
var metadata = await ReadGmcp(structured.Stream);
Check(metadata.StartsWith("Room.Info ", StringComparison.Ordinal) &&
    metadata.Contains("\"num\":\"tavern\"", StringComparison.Ordinal) &&
    metadata.Contains("\"id\":\"tavern\"", StringComparison.Ordinal),
    "GMCP room metadata matches authoritative current room");
await Send(structured.Stream, "north");
Check(await Read(structured.Stream) == "ROOM square", "Telnet direction works");
Check((await ReadGmcp(structured.Stream)).Contains("\"num\":\"square\"", StringComparison.Ordinal),
    "GMCP room follows same movement");
await Send(structured.Stream, "say MixedCASE stays Here");
Check(await Read(structured.Stream) == "SENT 1", "lowercase say verb works without changing payload");
await Send(structured.Stream, "poll 0");
Check(await Read(structured.Stream) == "MSG 1 alice: MixedCASE stays Here", "chat content case preserved");
Check(await Read(structured.Stream) == "END", "lowercase poll completes");
await structured.Stream.WriteAsync(GmcpSub("Core.Supports.Remove [\"Room\"]"));
await Send(structured.Stream, "look");
Check(await Read(structured.Stream) == "ROOM square", "text LOOK survives module removal");
await Send(structured.Stream, "ping");
Check(await Read(structured.Stream) == "PONG", "removed Room emits no interleaved frame");
await Send(structured.Stream, "quit");
Check(await Read(structured.Stream) == "BYE", "Telnet-mode logout");
structured.Stream.Dispose();
structured.Socket.Dispose();

var textOnly = await ConnectTelnet(telnetEndpoint, certificate);
await Send(textOnly.Stream, "login bob");
Check(await Read(textOnly.Stream) == "PASSWORD", "Telnet client may decline GMCP");
await Send(textOnly.Stream, "pass sample-bob-only-123!");
Check((await Read(textOnly.Stream)).StartsWith("SESSION ", StringComparison.Ordinal),
    "text-only authenticated session");
Check(await Read(textOnly.Stream) == "ROOM tavern", "text-only room");
await Send(textOnly.Stream, "ping");
Check(await Read(textOnly.Stream) == "PONG", "no metadata for non-GMCP client");
await Send(textOnly.Stream, "quit");
Check(await Read(textOnly.Stream) == "BYE", "text-only logout");
textOnly.Stream.Dispose();
textOnly.Socket.Dispose();

// Parser failures are connection-local: reject malformed negotiated GMCP
// without faulting the listener, then accept another valid client.
var invalidGmcp = await ConnectTelnet(telnetEndpoint, certificate);
await invalidGmcp.Stream.WriteAsync(new byte[] { 255, 253, 201 });
await invalidGmcp.Stream.WriteAsync(GmcpSub("Core.Supports.Set broken-json"));
bool malformedClosed = false;
try { _ = await Read(invalidGmcp.Stream); }
catch (Exception e) when (e is IOException ||
    e.Message == "Unexpected connection closure.") { malformedClosed = true; }
Check(malformedClosed, "malformed GMCP terminates only its own TLS session");
invalidGmcp.Stream.Dispose();
invalidGmcp.Socket.Dispose();
var afterMalformed = await ConnectTelnet(telnetEndpoint, certificate);
await Send(afterMalformed.Stream, "ping"); // Before login the server rejects commands.
bool preAuthClosed = false;
try { _ = await Read(afterMalformed.Stream); }
catch (Exception e) when (e is IOException ||
    e.Message == "Unexpected connection closure.") { preAuthClosed = true; }
Check(preAuthClosed, "listener remains available and does not accept unauthenticated gameplay");
afterMalformed.Stream.Dispose();
afterMalformed.Socket.Dispose();


// Session liveness has a distinct idle deadline: 250 ms of silence should NOT
// be treated as an incomplete command with a 150 ms progress budget.
var timedSessions = new RoomSessionCoordinator(new RoomChatRuntime(rooms), auth);
await using var timedServer = new LoopbackTlsGameServer(timedSessions, certificate,
    maxConnections: 2, preAuthReadTimeout: TimeSpan.FromMilliseconds(250),
    authenticatedIdleTimeout: TimeSpan.FromSeconds(3),
    incompleteCommandTimeout: TimeSpan.FromMilliseconds(150));
var timedEndpoint = timedServer.Start();
var timedAlice = await Connect(timedEndpoint, certificate);
await Login(timedAlice.Ssl, "alice", "sample-alice-only-123!");
await Task.Delay(450);
await Send(timedAlice.Ssl, "ping");
Check(await Read(timedAlice.Ssl) == "PONG",
    "authenticated idle session outlives incomplete-command deadline");
await timedAlice.Ssl.WriteAsync(Encoding.UTF8.GetBytes("pi"));
await Task.Delay(350);
bool slowClosed = false;
try { _ = await Read(timedAlice.Ssl); }
catch (Exception e) when (e is IOException || e.Message == "Unexpected connection closure.") { slowClosed = true; }
Check(slowClosed, "partial command cannot extend its fixed read deadline");
timedAlice.Ssl.Dispose();
timedAlice.Socket.Dispose();
var stalled = await Connect(timedEndpoint, certificate);
await Task.Delay(500);
bool stalledPreAuthClosed = false;
try { _ = await Read(stalled.Ssl); }
catch (Exception e) when (e is IOException || e.Message == "Unexpected connection closure.") { stalledPreAuthClosed = true; }
Check(stalledPreAuthClosed, "stalled unauthenticated client reaches shorter admission deadline");
stalled.Ssl.Dispose();
stalled.Socket.Dispose();
var timedBob = await Connect(timedEndpoint, certificate);
await Login(timedBob.Ssl, "bob", "sample-bob-only-123!");
await Send(timedBob.Ssl, "quit");
Check(await Read(timedBob.Ssl) == "BYE", "listener recovers after read deadlines");
timedBob.Ssl.Dispose();
timedBob.Socket.Dispose();

Console.WriteLine("Loopback TLS handshake, negative certificate, room chat, reconnect and live GMCP tests passed.");
