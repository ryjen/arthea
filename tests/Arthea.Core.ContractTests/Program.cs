using Arthea.Core.World;

// Dependency-free per-case harness. Runs in addition to the original smoke
// executable; no external test packages or runtime deployment dependencies.
var failures = 0;
var executed = 0;
void Case(string name, Action verify)
{
    executed++;
    try
    {
        verify();
        Console.WriteLine("PASS " + name);
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine("FAIL " + name + " (" + ex.GetType().Name + "): " + ex.Message);
    }
}
static void Check(bool result)
{
    if (!result) throw new InvalidOperationException("Contract assertion failed");
}
static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException("Expected " + typeof(T).Name);
}
static RoomGraph Graph() => new(new[]
{
    new RoomDefinition("tavern", "Tavern",
        new Dictionary<Direction, string> { [Direction.North] = "square" }),
    new RoomDefinition("square", "Square", new Dictionary<Direction, string>())
});
Case("directed edges do not imply reverse edges", () =>
{
    var graph = Graph();
    Check(graph.TryMove("tavern", Direction.North, out var destination) && destination == "square");
    Check(!graph.TryMove("square", Direction.South, out _));
});
Case("caller changes cannot mutate graph", () =>
{
    var exits = new Dictionary<Direction, string> { [Direction.North] = "square" };
    var graph = new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern", exits),
        new RoomDefinition("square", "Square", new Dictionary<Direction, string>())
    });
    exits[Direction.North] = "tavern";
    Check(graph.TryMove("tavern", Direction.North, out var destination) && destination == "square");
});
Case("unknown destinations fail closed", () =>
    Reject<ArgumentException>(() => new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern",
            new Dictionary<Direction, string> { [Direction.North] = "missing" })
    })));
Case("bidi controls are rejected in display metadata", () =>
    Reject<ArgumentException>(() => new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern\u202Espoof",
            new Dictionary<Direction, string>())
    })));
Case("safe supplementary Unicode is accepted", () =>
{
    var graph = new RoomGraph(new[]
    {
        new RoomDefinition("garden", "Garden 🌳", new Dictionary<Direction, string>())
    });
    Check(graph.TryGetRoom("garden", out var room) && room?.Name == "Garden 🌳");
});
Console.WriteLine($"Core contract cases: {executed} executed, {failures} failed.");
return failures == 0 ? 0 : 1;
