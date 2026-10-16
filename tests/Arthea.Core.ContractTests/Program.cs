using Arthea.Core.World;

// Dependency-free per-case harness. Runs in addition to the original smoke
// executable; no external test packages or runtime deployment dependencies.
var cases = new List<(string Name, Action Verify)>();
var failures = 0;
var executed = 0;
void Case(string name, Action verify) => cases.Add((name, verify));
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
// Topology is a detached, validated snapshot. The application has no live
// topology-editing API; a future editor must stage and validate a replacement.
Case("explicit reciprocal edges survive caller mutation", () =>
{
    var tavernExits = new Dictionary<Direction, string> { [Direction.East] = "square" };
    var squareExits = new Dictionary<Direction, string> { [Direction.West] = "tavern" };
    var graph = new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern", tavernExits),
        new RoomDefinition("square", "Square", squareExits),
    });
    tavernExits.Clear();
    squareExits.Clear();
    Check(graph.TryMove("tavern", Direction.East, out var toSquare) && toSquare == "square");
    Check(graph.TryMove("square", Direction.West, out var toTavern) && toTavern == "tavern");
});
Case("published room exit collection rejects mutation", () =>
{
    var graph = Graph();
    Check(graph.TryGetRoom("tavern", out var room) && room is not null);
    var writableView = room!.Exits as IDictionary<Direction, string>;
    Check(writableView is not null);
    Reject<NotSupportedException>(() => writableView![Direction.South] = "square");
    Check(!graph.TryMove("tavern", Direction.South, out _));
    Check(graph.TryMove("tavern", Direction.North, out var north) && north == "square");
});
Case("rejected replacement snapshot leaves published topology unchanged", () =>
{
    var published = Graph();
    // A half-edited candidate with a dangling endpoint must fail in its own
    // constructor; there is no mutable published graph to roll back.
    Reject<ArgumentException>(() => new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern",
            new Dictionary<Direction, string> { [Direction.North] = "square" }),
        new RoomDefinition("square", "Square",
            new Dictionary<Direction, string> { [Direction.South] = "not-a-room" }),
    }));
    Check(published.TryMove("tavern", Direction.North, out var target) && target == "square");
    Check(!published.TryMove("square", Direction.South, out _));
});
if (args.Length == 1 && args[0] == "--list")
{
    foreach (var item in cases) Console.WriteLine(item.Name);
    return 0;
}
if (args.Length != 0 && (args.Length != 2 || args[0] != "--case" ||
    !cases.Any(item => item.Name == args[1])))
{
    Console.Error.WriteLine("Usage: Arthea.Core.ContractTests [--list | --case <exact name>]");
    return 2;
}
foreach (var (name, verify) in cases)
{
    if (args.Length == 2 && name != args[1]) continue;
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
Console.WriteLine($"Core contract cases: {executed} executed, {failures} failed.");
return failures == 0 ? 0 : 1;
