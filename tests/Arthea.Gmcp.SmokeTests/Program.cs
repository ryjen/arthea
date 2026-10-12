using System.Text;
using System.Text.Json;
using Arthea.Core.World;
using Arthea.Transport;

static void Check(bool pass, string label)
{
    if (!pass) throw new Exception("GMCP contract failed: " + label);
}
static void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}: {label}");
}
static byte[] Sub(string text) =>
    [TelnetGmcpCodec.Iac, TelnetGmcpCodec.Sb, TelnetGmcpCodec.Gmcp,
     ..Encoding.UTF8.GetBytes(text), TelnetGmcpCodec.Iac, TelnetGmcpCodec.Se];
var rooms = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "A \"Quoted\" Inn",
        new Dictionary<Direction,string> { [Direction.North] = "square",
                                           [Direction.Down] = "cellar" }),
    new RoomDefinition("square", "Square", new Dictionary<Direction,string>()),
    new RoomDefinition("cellar", "Cellar", new Dictionary<Direction,string>())
});
var plain = new TelnetGmcpCodec();
Check(TelnetGmcpCodec.OfferGmcp().SequenceEqual(new byte[] { 255, 251, 201 }),
    "server WILL GMCP wire offer");
Check(plain.EncodeRoomInfo(rooms, "tavern") is null, "plaintext clients get no metadata");
var text = plain.Feed(Encoding.UTF8.GetBytes("LO"));
Check(text.Lines.Count == 0, "fragment is buffered");
text = plain.Feed(Encoding.UTF8.GetBytes("OK\r\nSAY hi\n"));
Check(text.Lines.SequenceEqual(new[] { "LOOK", "SAY hi" }), "fragmented lines preserved");
Check(!plain.SupportsRoomInfo, "metadata disabled without DO + supports");
var unrecognized = plain.Feed(new byte[] { 255, 253, 44 }); // DO unsupported option
Check(unrecognized.Replies.Count == 1 &&
    unrecognized.Replies[0].SequenceEqual(new byte[] { 255, 252, 44 }),
    "unsupported Telnet DO rejected with WONT");

var client = new TelnetGmcpCodec();
client.Feed(new byte[] { 255 });
client.Feed(new byte[] { 253 });
client.Feed(new byte[] { 201 });
Check(client.Enabled, "fragmented DO GMCP accepted");
var supports = Sub("Core.Supports.Set [\"Room 1\", \"Char.Vitals 1\"]");
client.Feed(supports.AsSpan(0, 7));
client.Feed(supports.AsSpan(7));
Check(client.SupportsRoomInfo, "Core.Supports.Set enables Room");
var frame = client.EncodeRoomInfo(rooms, "tavern")!;
Check(frame[0] == 255 && frame[1] == 250 && frame[2] == 201 &&
    frame[^2] == 255 && frame[^1] == 240, "Room.Info GMCP wire envelope");
var raw = Encoding.UTF8.GetString(frame.AsSpan(3, frame.Length - 5));
Check(raw.StartsWith("Room.Info ", StringComparison.Ordinal), "Room.Info package");
using (var doc = JsonDocument.Parse(raw[10..]))
{
    Check(doc.RootElement.GetProperty("id").GetString() == "tavern", "stable room id");
    Check(doc.RootElement.GetProperty("name").GetString() == "A \"Quoted\" Inn",
        "room name escaped in JSON");
    Check(doc.RootElement.GetProperty("exits").GetProperty("n").GetString() == "square" &&
          doc.RootElement.GetProperty("exits").GetProperty("d").GetString() == "cellar",
        "one-way and vertical exit IDs");
}
client.Feed(Sub("Core.Supports.Remove [\"Room\"]"));
Check(!client.SupportsRoomInfo, "remove disables Room metadata");
client.Feed(Sub("Core.Supports.Add [\"Room.Info 1\"]"));
Check(client.SupportsRoomInfo, "add enables specific room metadata");
client.Feed(new byte[] { 255, 254, 201 });
Check(!client.Enabled && client.EncodeRoomInfo(rooms, "tavern") is null,
    "DONT GMCP removes option and metadata");
client.Feed(new byte[] { 255, 253, 201 });
Check(!client.SupportsRoomInfo, "re-enable requires fresh capability advertisement");
client.Feed(Sub("Core.Supports.Set [\"Room 1\"]"));
Check(client.SupportsRoomInfo, "fresh advert re-enables metadata");

Reject<InvalidDataException>(() => client.Feed(Sub("Core.Supports.Set [\"Room 1\",\"Char 0\"]")),
    "invalid capability batch rejected as a unit");
Check(client.SupportsRoomInfo, "invalid Set cannot replace prior negotiated capabilities");
Reject<InvalidDataException>(() => client.Feed(Sub("Core.Supports.Set [\"Room 0\"]")),
    "invalid version rejected");
Reject<InvalidDataException>(() => client.Feed(Sub("Core.Supports.Set nope")),
    "invalid JSON rejected");
Reject<InvalidDataException>(() => client.Feed(
    [255, 250, 201, ..Enumerable.Repeat((byte)'x', 4097)]),
    "oversized GMCP rejected");
Reject<InvalidDataException>(() => new TelnetGmcpCodec().Feed(
    Encoding.UTF8.GetBytes(new string('z', 1025))), "command limit");
Reject<InvalidDataException>(() => new TelnetGmcpCodec().Feed(new byte[] { 0, 10 }),
    "NUL byte rejected");
Reject<InvalidDataException>(() => new TelnetGmcpCodec().Feed(new byte[] { 255, 255 }),
    "raw IAC rejected");
Check(new TelnetGmcpCodec().Feed(Sub("Core.Supports.Set [\"Room 1\"]")).Lines.Count == 0,
    "GMCP bytes ignored without negotiation");
var oversizedFragments = new TelnetGmcpCodec();
oversizedFragments.Feed(new byte[] { 255, 253, 201, 255, 250, 201 });
for (var i = 0; i < 4; i++)
    oversizedFragments.Feed(Enumerable.Repeat((byte)'x', 1024).ToArray());
Reject<InvalidDataException>(() => oversizedFragments.Feed(new byte[] { (byte)'z' }),
    "GMCP subnegotiation limit holds across separate chunks");
Check(new TelnetGmcpCodec().Feed([255, 250, 42, 1, 2, 255, 240]).Lines.Count == 0,
    "unknown subnegotiation ignored");

Console.WriteLine("Telnet/GMCP framing, fragmentation, capability gating and room metadata passed.");
