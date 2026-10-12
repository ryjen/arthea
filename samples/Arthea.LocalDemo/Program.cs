using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Arthea.Core.Social;
using Arthea.Core.World;
using Arthea.Transport;

// Opt-in local fixture only; no general listening endpoint, configurable bind
// address, real account enrollment, background service or durable game state.
if (args.Length != 1 || args[0] is not ("--local-demo" or "--self-check"))
{
    Console.Error.WriteLine("Usage: Arthea.LocalDemo --self-check | --local-demo");
    Console.Error.WriteLine("Local demonstration only. No public Internet hosting.");
    return 2;
}

using var key = RSA.Create(2048);
var certificateRequest = new CertificateRequest("CN=localhost", key,
    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
certificateRequest.CertificateExtensions.Add(
    new X509BasicConstraintsExtension(false, false, 0, false));
certificateRequest.CertificateExtensions.Add(
    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
certificateRequest.CertificateExtensions.Add(
    new X509SubjectKeyIdentifierExtension(certificateRequest.PublicKey, false));
var san = new SubjectAlternativeNameBuilder();
san.AddDnsName("localhost");
san.AddIpAddress(IPAddress.Loopback);
certificateRequest.CertificateExtensions.Add(san.Build());
using var certificate = certificateRequest.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(2));

var graph = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "The Rusty Lantern",
        new Dictionary<Direction, string>
        {
            [Direction.North] = "square",
            [Direction.Down] = "cellar"
        }),
    new RoomDefinition("square", "Market Square",
        new Dictionary<Direction, string> { [Direction.South] = "tavern" }),
    new RoomDefinition("cellar", "Cellar", new Dictionary<Direction, string>())
});

// These are disposable literal example credentials, deliberately unrelated to
// any legacy user, real account, shared secret, or deployment configuration.
const string alicePassword = "arthea-demo-alice-ONLY-2026!";
const string bobPassword = "arthea-demo-bob-ONLY-2026!";
var auth = new PasswordAccountAuthenticator(new[]
{
    PasswordAccountAuthenticator.Provision("alice", alicePassword),
    PasswordAccountAuthenticator.Provision("bob", bobPassword)
});
var coordinator = new RoomSessionCoordinator(
    new RoomChatRuntime(graph, retainedPerRoom: 20), auth,
    sessionLifetime: TimeSpan.FromMinutes(15), maxSessions: 2);

await using var server = new LoopbackTlsGameServer(coordinator, certificate,
    maxConnections: 4, enableTelnetGmcp: true);
var endpoint = server.Start();
if (!IPAddress.IsLoopback(endpoint.Address))
    throw new InvalidOperationException("Refusing non-loopback address.");

if (args[0] == "--self-check")
{
    Console.WriteLine("Local TLS demo bind/self-check passed.");
    return 0;
}

Console.WriteLine("ARThea LOCAL TEST ONLY — never expose this endpoint to a network.");
Console.WriteLine($"Address: 127.0.0.1:{endpoint.Port} (TLS only)");
Console.WriteLine($"Certificate SHA-256: {certificate.GetCertHashString(HashAlgorithmName.SHA256)}");
Console.WriteLine("Use a TLS-capable desktop Telnet client with an explicit, local-only");
Console.WriteLine("trust decision for this ephemeral self-signed certificate.");
Console.WriteLine("Type: LOGIN alice  /  PASS arthea-demo-alice-ONLY-2026!");
Console.WriteLine("Or:   LOGIN bob    /  PASS arthea-demo-bob-ONLY-2026!");
Console.WriteLine("Available commands: LOOK NORTH SOUTH UP DOWN SAY <text> POLL 0 PING QUIT");
Console.WriteLine("Session bearer output is prototype-only; do not save or share it.");
Console.WriteLine("Ctrl+C ends the demo. All sessions and chat history are discarded.");
var exit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
ConsoleCancelEventHandler cancelHandler = (_, e) =>
{
    e.Cancel = true;
    exit.TrySetResult();
};
Console.CancelKeyPress += cancelHandler;
try { await exit.Task; }
finally { Console.CancelKeyPress -= cancelHandler; }
return 0;
