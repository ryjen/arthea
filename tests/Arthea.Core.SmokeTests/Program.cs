using Arthea.Core.World;

static void Assert(bool condition, string description)
{
    if (!condition) throw new Exception($"Failed: {description}");
}

static void AssertThrows<TException>(Action action, string description)
    where TException : Exception
{
    try { action(); }
    catch (TException) { return; }
    throw new Exception($"Expected {typeof(TException).Name}: {description}");
}

var tavernExits = new Dictionary<Direction, string>
{
    [Direction.North] = "square",
    [Direction.Down] = "cellar",
};
var graph = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "The Rusty Lantern", tavernExits),
    new RoomDefinition("square", "Market Square",
        new Dictionary<Direction, string> { [Direction.South] = "tavern" }),
    new RoomDefinition("cellar", "Cellar", new Dictionary<Direction, string>()),
});
Assert(graph.RoomCount == 3, "all three rooms exist");
Assert(graph.TryMove("tavern", Direction.North, out var north) && north == "square", "north exit");
Assert(graph.TryMove("tavern", Direction.Down, out var down) && down == "cellar", "vertical exit");
Assert(!graph.TryMove("cellar", Direction.Up, out _), "one-way exit does not imply its reverse");
Assert(!graph.TryMove("missing", Direction.North, out _), "unknown room cannot move");
Assert(!graph.TryMove("tavern", Direction.West, out _), "unknown exit cannot move");
Assert(graph.TryGetRoom("tavern", out var tavern) && tavern?.Name == "The Rusty Lantern", "room description available");

// Mutation of caller-owned dictionaries cannot change the authoritative snapshot.
tavernExits[Direction.North] = "cellar";
Assert(graph.TryMove("tavern", Direction.North, out north) && north == "square", "input map defensively copied");

AssertThrows<ArgumentException>(() => new RoomGraph(new[]
{
    new RoomDefinition("r", "One", new Dictionary<Direction, string>()),
    new RoomDefinition("r", "Two", new Dictionary<Direction, string>()),
}), "duplicate IDs");
AssertThrows<ArgumentException>(() => new RoomGraph(new[]
{
    new RoomDefinition("r", "One",
        new Dictionary<Direction, string> { [Direction.North] = "unknown" }),
}), "dangling reference");
AssertThrows<ArgumentException>(() => new RoomGraph(new[]
{
    new RoomDefinition("r", "One",
        new Dictionary<Direction, string> { [(Direction)99] = "r" }),
}), "invalid legacy direction");
Assert((int)Direction.North == 0 && (int)Direction.South == 1
    && (int)Direction.East == 2 && (int)Direction.West == 3
    && (int)Direction.Up == 4 && (int)Direction.Down == 5, "legacy enum numeric contract");
Assert(!graph.TryGetRoom("not-here", out _), "missing room lookup returns false");
AssertThrows<ArgumentNullException>(() => new RoomGraph(null!), "null graph input");
AssertThrows<ArgumentException>(() => new RoomGraph(new[] {
    new RoomDefinition("", "Nameless room id", new Dictionary<Direction, string>())
}), "empty room ID");
AssertThrows<ArgumentException>(() => new RoomGraph(new[] {
    new RoomDefinition("r", "", new Dictionary<Direction, string>())
}), "empty room name");


// Room IDs are rendered into line-oriented ROOM replies; reject crafted
// metadata before it can cross the transport or persistence boundary.
static void BadRoom(string id, string name, string label) =>
    AssertThrows<ArgumentException>(() => new RoomGraph(new[]
    {
        new RoomDefinition(id, name, new Dictionary<Direction, string>())
    }), label);
BadRoom("tavern\nSESSION forged", "Tavern", "embedded newline in room ID");
BadRoom("tavern\rROOM fake", "Tavern", "carriage return in room ID");
BadRoom("tavern\u001b[31m", "Tavern", "ANSI escape in room ID");
BadRoom("tavern\u202e", "Tavern", "bidirectional override in room ID");
BadRoom("tavern\u2028", "Tavern", "Unicode line separator in room ID");
BadRoom("tavern\uD800", "Tavern", "unpaired high surrogate in room ID");
BadRoom("tavern", "Tavern\uDC00", "unpaired low surrogate in room name");
BadRoom(new string('r', 257), "Tavern", "oversize room ID");
BadRoom("tavern", "Tavern\nSESSION forged", "room name newline");
BadRoom("tavern", "Tavern\u001b[2J", "room name terminal escape");
BadRoom("tavern", new string('n', 257), "oversize room name");
AssertThrows<ArgumentException>(() => new RoomGraph(new[]
{
    new RoomDefinition("tavern", "Tavern",
        new Dictionary<Direction, string> { [Direction.North] = "square\nROOM forged" })
}), "unsafe exit destination rejected before dangling-ref validation");
var unicodeRoom = new RoomGraph(new[]
{
    new RoomDefinition("café", "Café du monde",
        new Dictionary<Direction, string>())
});
Assert(unicodeRoom.TryGetRoom("café", out _), "safe Unicode metadata remains supported");
var supplementary = new RoomGraph(new[]
{
    new RoomDefinition("garden", "Garden 🌳",
        new Dictionary<Direction, string>())
});
Assert(supplementary.TryGetRoom("garden", out _),
    "valid supplementary-plane Unicode metadata remains supported");

// Reproducible graph-generation checks: edges always resolve to declared rooms,
// and mutating any caller-owned exit map after construction cannot affect it.
var random = new Random(0xA47E);
for (var iteration = 0; iteration < 64; iteration++)
{
    var count = random.Next(1, 24);
    var input = new List<RoomDefinition>(count);
    var sourceMaps = new List<Dictionary<Direction, string>>(count);
    var directionValues = Enum.GetValues<Direction>();
    for (var i = 0; i < count; i++)
    {
        var exits = new Dictionary<Direction, string>();
        foreach (var direction in directionValues)
            if (random.Next(3) == 0)
                exits[direction] = "room-" + random.Next(count);
        sourceMaps.Add(exits);
        input.Add(new RoomDefinition("room-" + i, "Room " + i, exits));
    }
    var generated = new RoomGraph(input);
    Assert(generated.RoomCount == count, "generated graph node cardinality");
    for (var i = 0; i < count; i++)
    {
        var id = "room-" + i;
        foreach (var direction in directionValues)
        {
            var expected = sourceMaps[i].TryGetValue(direction, out var target);
            Assert(generated.TryMove(id, direction, out var actual) == expected &&
                   (!expected || actual == target),
                "generated graph directional lookup");
        }
        sourceMaps[i].Clear();
    }
    for (var i = 0; i < count; i++)
    {
        var room = input[i];
        Assert(generated.TryGetRoom(room.Id, out var detached) && detached is not null &&
               detached.Exits.Count <= 6, "generated graph detached room metadata");
        foreach (var (direction, target) in detached!.Exits)
            Assert(generated.TryMove(room.Id, direction, out var actual) &&
                   actual == target && generated.TryGetRoom(target, out _),
                "generated graph valid frozen edges");
    }
}

// Legacy persistence extraction contract (not a MongoDB driver integration).
var legacy = new[]
{
    new Arthea.LegacyBridge.LegacyRoomSnapshot("a", "Tavern",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot>
        {
            ["North"] = new("b", "South"), // Legacy link editor may leave mismatched stored field.
            ["DOWN"] = new("c", null),
        }),
    new Arthea.LegacyBridge.LegacyRoomSnapshot("b", "Square",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot>()),
    new Arthea.LegacyBridge.LegacyRoomSnapshot("c", "Cellar",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot>()),
};
var projected = Arthea.LegacyBridge.LegacyRoomProjection.Build(legacy);
Assert(projected.RoomCount == 3, "legacy projection retains rooms");
Assert(projected.TryMove("a", Direction.North, out var projectedNorth) && projectedNorth == "b",
    "dictionary direction is authoritative over stored Exit.Direction");
Assert(projected.TryMove("a", Direction.Down, out var projectedDown) && projectedDown == "c",
    "case-insensitive canonical direction name");
Assert(!projected.TryMove("b", Direction.South, out _),
    "one-way legacy link must not synthesize a reverse edge");
AssertThrows<ArgumentException>(() => Arthea.LegacyBridge.LegacyRoomProjection.Build(new[]
{
    new Arthea.LegacyBridge.LegacyRoomSnapshot("a", "Tavern",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot> { ["North"] = new("missing", null) })
}), "dangling legacy destination fails closed");
AssertThrows<ArgumentException>(() => Arthea.LegacyBridge.LegacyRoomProjection.Build(new[]
{
    new Arthea.LegacyBridge.LegacyRoomSnapshot("a", "Tavern",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot> { ["0"] = new("a", null) })
}), "numeric direction key is rejected as ambiguous");
AssertThrows<ArgumentException>(() => Arthea.LegacyBridge.LegacyRoomProjection.Build(new[]
{
    new Arthea.LegacyBridge.LegacyRoomSnapshot("a", "Tavern",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot>
        {
            ["North"] = new("a", null),
            ["north"] = new("a", null)
        })
}), "duplicate keys after direction normalization are rejected");
AssertThrows<ArgumentException>(() => Arthea.LegacyBridge.LegacyRoomProjection.Build(new[]
{
    new Arthea.LegacyBridge.LegacyRoomSnapshot("a", "Tavern",
        new Dictionary<string, Arthea.LegacyBridge.LegacyExitSnapshot>
        {
            ["South"] = new(null, null)
        })
}), "missing destination is rejected");

Console.WriteLine("Arthea.Core room topology and legacy projection smoke tests passed.");
