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

Console.WriteLine("Arthea.Core room topology smoke tests passed.");
