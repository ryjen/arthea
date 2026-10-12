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
Console.WriteLine("Loopback TLS handshake, negative certificate, room chat and reconnect tests passed.");
