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
