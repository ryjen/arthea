using Arthea.Core.World;
using Arthea.LegacyBson;
using MongoDB.Bson;

static void Check(bool condition, string label)
{
    if (!condition) throw new Exception($"BSON contract failed: {label}");
}
static void Rejected<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected rejection ({typeof(T).Name}): {label}");
}
static BsonDocument Room(string id, string name, BsonDocument exits) =>
    new() { { "_id", id }, { "Name", name }, { "Exits", exits } };

var tavern = Room("tavern", "The Rusty Lantern", new BsonDocument
{
    { "North", new BsonDocument { { "ToRoomRef", "square" }, { "Direction", 1 },
        { "RoomRef", "tavern" } } },
    { "Down", new BsonDocument { { "ToRoomRef", "cellar" } } },
});
var square = Room("square", "Market Square", new BsonDocument
{
    { "South", new BsonDocument { { "ToRoomRef", "tavern" }, { "Direction", 1 } } }
});
var cellar = Room("cellar", "Cellar", new BsonDocument());

var bytes = new[] { tavern.ToBson(), square.ToBson(), cellar.ToBson() };
var graph = LegacyBsonRoomReader.ReadGraph(bytes);
Check(graph.RoomCount == 3, "3 encoded BSON room documents imported");
Check(graph.TryMove("tavern", Direction.North, out var north) && north == "square",
    "BSON key controls direction despite mismatched stored field");
Check(graph.TryMove("tavern", Direction.Down, out var down) && down == "cellar",
    "vertical exit survives BSON read");
Check(!graph.TryMove("cellar", Direction.Up, out _), "no synthesized reverse exit");

// Synthetic test documents are truly binary BSON, not Extended JSON or text maps.
Check(bytes[0].Length >= 5 && BitConverter.ToInt32(bytes[0], 0) == bytes[0].Length,
    "BSON wire framing is present");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(Array.Empty<byte>()), "empty BSON");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(new byte[LegacyBsonRoomReader.MaxRoomBytes + 1]), "oversized BSON");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(new byte[] { 1, 2, 3 }), "malformed BSON");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(bytes[0].Concat(new byte[] { 0 }).ToArray()), "trailing bytes");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(
    Room("tavern", "Tavern", new BsonDocument()).Set("_id", new BsonObjectId(ObjectId.GenerateNewId())).ToBson()),
    "unproven ObjectId storage coercion");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(
    new BsonDocument { { "_id", "x" }, { "Name", "X" } }.ToBson()),
    "missing explicit Exits");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(
    Room("x", "X", new BsonDocument { { "North", new BsonString("unsafe") } }).ToBson()),
    "exit must be document");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(
    Room("x", "X", new BsonDocument { { "North", new BsonDocument { { "ToRoomRef", BsonNull.Value } } } }).ToBson()),
    "null destination");
Rejected<ArgumentException>(() => LegacyBsonRoomReader.ReadGraph(new[]
{
    Room("x", "X", new BsonDocument { { "North", new BsonDocument { { "ToRoomRef", "gone" } } } }).ToBson()
}), "missing target room fails closed");
Rejected<ArgumentException>(() => LegacyBsonRoomReader.ReadGraph(new[]
{
    Room("x", "X", new BsonDocument { { "0", new BsonDocument { { "ToRoomRef", "x" } } } }).ToBson()
}), "ambiguous numeric direction rejected");
Rejected<InvalidDataException>(() => LegacyBsonRoomReader.ReadRoom(
    Room("x", new string('x', 1025), new BsonDocument()).ToBson()),
    "oversized room name");

Console.WriteLine("Binary BSON room import and negative tests passed.");
