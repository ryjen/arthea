using ArtheaEngine;
using ArtheaEngine.Model;
using ArtheaServer;
using ArtheaServer.Creation;
using Arthea.LegacyBridge;
using Arthea.Core.World;

static void Check(bool value, string observation)
{
    if (!value) throw new Exception("Original exit editor characterization failed: " + observation);
}

// The editor and tokenizer are the original files. The room, database and
// transport objects are synthetic and exercise no historic storage or network.
Room.Clear();
var origin = new Room { Id = "origin", Name = "Origin" };
var destination = new Room { Id = "destination", Name = "Destination" };
Room.Register(origin);
Room.Register(destination);
var editor = new ExitsEditor(new TelnetConnection(), origin, typeof(Room).GetProperty(nameof(Room.Exits))!);

// Original 'link' accepts the abbreviated command and mixed-case direction.
// It adds only the outgoing dictionary entry; no return edge is created.
Check(editor.Execute(new Argument("L wEsT destination")), "original link editor handles input");
Check(origin.Exits.TryGetValue(ArtheaEngine.Model.Direction.West, out var oneWay) &&
      ReferenceEquals(oneWay.ToRoom, destination) &&
      ReferenceEquals(oneWay.Room, origin), "one-way link points at the right room");
Check(destination.Exits.Count == 0, "one-way link does not create reverse exit");
// The original link path does NOT assign Exit.Direction: it stays the enum
// default North even while the authoritative dictionary key is West.
Check(oneWay.Direction == ArtheaEngine.Model.Direction.North,
    "link preserves mismatched default stored Direction field");

// Project the legacy-shape snapshot through the REAL modern bridge, not a
// duplicate projection. Only the dictionary key controls movement.
var projected = LegacyRoomProjection.Build(new[]
{
    new LegacyRoomSnapshot(origin.Id, origin.Name, new Dictionary<string, LegacyExitSnapshot>
    {
        [nameof(ArtheaEngine.Model.Direction.West)] =
            new(destination.Id, oneWay.Direction.ToString()),
    }),
    new LegacyRoomSnapshot(destination.Id, destination.Name,
        new Dictionary<string, LegacyExitSnapshot>()),
});
Check(projected.TryMove(origin.Id, Arthea.Core.World.Direction.West, out var moveTarget) &&
      moveTarget == destination.Id, "modern graph follows authoritative legacy map key");
Check(!projected.TryMove(destination.Id, Arthea.Core.World.Direction.East, out _),
    "modern graph does not invent a reverse edge");

// Original TryParse accepts a defined direction even in different case.
// Duplicate entries do not replace a pre-existing link.
Check(editor.Execute(new Argument("link WEST destination")), "duplicate link command handled");
Check(origin.Exits.Count == 1, "duplicate link does not add a second edge");
Check(editor.Execute(new Argument("link nonsense destination")), "malformed direction command handled");
Check(origin.Exits.Count == 1, "unrecognized direction not persisted");
Check(editor.Execute(new Argument("link East missing")), "missing destination command handled");
Check(origin.Exits.Count == 1, "unresolved destination not persisted");

// Critical legacy quirk: Enum.TryParse permits unrecognized numeric enum
// values. In this synthetic lookup fixture, the original link editor can
// store such a key, which the modern bridge must reject.
var unsupportedDirectionValue =
    Enum.GetValues<ArtheaEngine.Model.Direction>().Max(value => (int)value) + 1;
var unsupportedDirectionText =
    unsupportedDirectionValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
Check(editor.Execute(new Argument("link " + unsupportedDirectionText + " destination")), "numeric direction is accepted by editor");
Check(origin.Exits.ContainsKey((ArtheaEngine.Model.Direction)unsupportedDirectionValue),
    "invalid numeric direction inserted into legacy dictionary");
var invalidLegacy = new[]
{
    new LegacyRoomSnapshot(origin.Id, origin.Name,
        new Dictionary<string, LegacyExitSnapshot>
        {
            [unsupportedDirectionText] = new(destination.Id, null),
        }),
    new LegacyRoomSnapshot(destination.Id, destination.Name,
        new Dictionary<string, LegacyExitSnapshot>()),
};
var failedClosed = false;
try { _ = LegacyRoomProjection.Build(invalidLegacy); }
catch (ArgumentException) { failedClosed = true; }
Check(failedClosed, "modern bridge rejects unsupported numeric direction");

// Deleting a specific outgoing link affects only the source room map.
Check(editor.Execute(new Argument("D West")), "delete alias handled");
Check(!origin.Exits.ContainsKey(ArtheaEngine.Model.Direction.West),
    "deleted direction removed from outgoing map");
Check(origin.Exits.ContainsKey((ArtheaEngine.Model.Direction)unsupportedDirectionValue),
    "delete does not accidentally clear unrelated links");
Check(destination.Exits.Count == 0, "delete leaves destination unchanged");
Check(editor.Execute(new Argument("delete West")), "missing deletion handled");
Check(origin.Exits.Count == 1, "missing deletion leaves remaining links unchanged");

// Confirm the test-only reverse map matches the original six-case method
// in Extensions.cs instead of silently treating the test copy as authority.
var originalExtensions = File.ReadAllText(
    Path.Combine(AppContext.BaseDirectory, "OriginalExtensions.cs.txt"));
var reversePairs = new (ArtheaEngine.Model.Direction Outbound,
    ArtheaEngine.Model.Direction Inbound)[]
{
    (ArtheaEngine.Model.Direction.North, ArtheaEngine.Model.Direction.South),
    (ArtheaEngine.Model.Direction.South, ArtheaEngine.Model.Direction.North),
    (ArtheaEngine.Model.Direction.East, ArtheaEngine.Model.Direction.West),
    (ArtheaEngine.Model.Direction.West, ArtheaEngine.Model.Direction.East),
    (ArtheaEngine.Model.Direction.Up, ArtheaEngine.Model.Direction.Down),
    (ArtheaEngine.Model.Direction.Down, ArtheaEngine.Model.Direction.Up),
};
Check(reversePairs.Length == Enum.GetValues<ArtheaEngine.Model.Direction>().Length,
    "all original enum directions included in mirror validation");
foreach (var (outbound, inbound) in reversePairs)
{
    Check(outbound.Reverse() == inbound && inbound.Reverse() == outbound,
        "test-only reverse mirror is symmetric for " + outbound);
    var originalBranch = "case Direction." + outbound +
        ": return Direction." + inbound + ";";
    Check(originalExtensions.Contains(originalBranch, StringComparison.Ordinal),
        "mirror agrees with unmodified original Extensions.cs for " + outbound);
}

// Keep each original-editor operation on a fresh synthetic room pair.
// Construction counts are test instrumentation only, not a persistence trace.
static (Room Source, Room Target, ExitsEditor Editor) NewEditor()
{
    Room.Clear();
    var source = new Room { Id = "source", Name = "Source" };
    var target = new Room { Id = "target", Name = "Target" };
    Room.Register(source);
    Room.Register(target);
    return (source, target,
        new ExitsEditor(new TelnetConnection(), source,
            typeof(Room).GetProperty(nameof(Room.Exits))!));
}

// 'add' creates a forward and reverse link with matching stored directions.
// The original branch then falls through instead of returning true: this is
// observable and should NOT be replicated in a new UI/dispatcher contract.
var (addSource, addTarget, addEditor) = NewEditor();
Check(!addEditor.Execute(new Argument("A eAsT target")),
    "original add mutates two rooms yet reports false");
Check(addSource.Exits.Count == 1 && addTarget.Exits.Count == 1,
    "add creates exactly one outgoing and one reverse link");
Check(addSource.Exits.TryGetValue(ArtheaEngine.Model.Direction.East, out var forward) &&
      ReferenceEquals(forward.Room, addSource) &&
      ReferenceEquals(forward.ToRoom, addTarget) &&
      forward.Direction == ArtheaEngine.Model.Direction.East,
    "add forward exit retains source, target and direction");
Check(addTarget.Exits.TryGetValue(ArtheaEngine.Model.Direction.West, out var backward) &&
      ReferenceEquals(backward.Room, addTarget) &&
      ReferenceEquals(backward.ToRoom, addSource) &&
      backward.Direction == ArtheaEngine.Model.Direction.West,
    "add reverse exit retains target, source and opposite direction");
Check(addEditor.Execute(new Argument("add East target")),
    "duplicate outbound add is handled");
Check(addSource.Exits.Count == 1 && addTarget.Exits.Count == 1,
    "duplicate outbound add leaves both maps unchanged");

// The editor rejects an occupied reverse direction before changing the
// source: no replacement or partial two-way graph is permitted by that path.
var (occupiedSource, occupiedTarget, occupiedEditor) = NewEditor();
occupiedTarget.Exits[ArtheaEngine.Model.Direction.West] = new Exit
{
    Room = occupiedTarget, ToRoom = occupiedTarget,
    Direction = ArtheaEngine.Model.Direction.West,
};
Check(occupiedEditor.Execute(new Argument("add East target")),
    "occupied reverse direction returns handled");
Check(occupiedSource.Exits.Count == 0 && occupiedTarget.Exits.Count == 1,
    "occupied reverse direction leaves the source empty");
Check(occupiedEditor.Execute(new Argument("add East unknown")),
    "missing target is handled before edits");
Check(occupiedSource.Exits.Count == 0, "missing target does not add an exit");
Check(occupiedEditor.Execute(new Argument("add not-a-direction target")),
    "unparseable add direction is handled");
Check(occupiedSource.Exits.Count == 0,
    "unparseable add direction leaves source unchanged");

// Numeric Enum.TryParse accepts a value outside the declared enum; unlike
// 'link', the two-way 'add' path calls Reverse(), which throws before writes.
var invalidNumeric = unsupportedDirectionText; // computed from enum, not a magic number
var (badSource, badTarget, badEditor) = NewEditor();
var addFailed = false;
try { _ = badEditor.Execute(new Argument("add " + invalidNumeric + " target")); }
catch (Exception error) when (error.Message == "Unknown direction for reverse")
{
    addFailed = true;
}
Check(addFailed, "unsupported numeric add direction throws in original reverse mapping");
Check(badSource.Exits.Count == 0 && badTarget.Exits.Count == 0,
    "invalid numeric add does not mutate either map");

// 'build' creates a new synthetic room and linked pair, but no real save or
// world registration is invoked in the original editor method.
var (buildSource, buildTarget, buildEditor) = NewEditor();
var roomsBeforeBuild = Room.CreatedRooms.Count;
Check(buildEditor.Execute(new Argument("B Up")),
    "build abbreviation is handled");
Check(Room.CreatedRooms.Count == roomsBeforeBuild + 1,
    "build constructs exactly one new room in this fixture");
var generatedRoom = Room.CreatedRooms[^1];
Check(buildSource.Exits.Count == 1 && buildTarget.Exits.Count == 0,
    "build only changes source and new room, not existing target");
Check(buildSource.Exits.TryGetValue(ArtheaEngine.Model.Direction.Up, out var builtExit) &&
      ReferenceEquals(builtExit.Room, buildSource) &&
      ReferenceEquals(builtExit.ToRoom, generatedRoom) &&
      builtExit.Direction == ArtheaEngine.Model.Direction.Up,
    "build installs the upward outgoing direction");
Check(generatedRoom.Exits.TryGetValue(ArtheaEngine.Model.Direction.Down, out var returnExit) &&
      ReferenceEquals(returnExit.Room, generatedRoom) &&
      ReferenceEquals(returnExit.ToRoom, buildSource) &&
      returnExit.Direction == ArtheaEngine.Model.Direction.Down,
    "build creates the downward reverse exit");
Check(Room.Lookup(generatedRoom.Id) is null,
    "original build handler did not register or persist the synthetic room");
Check(buildEditor.Execute(new Argument("build Up")), "duplicate build direction handled");
Check(Room.CreatedRooms.Count == roomsBeforeBuild + 1,
    "duplicate build does not construct another room");
Check(buildEditor.Execute(new Argument("build not-a-direction")),
    "invalid build direction syntax handled");
Check(Room.CreatedRooms.Count == roomsBeforeBuild + 1,
    "invalid syntax does not construct a new room");

// Invalid numeric 'build' constructs a transient room BEFORE it tries to
// reverse the unsupported direction and throws: observable partial work.
var (badBuildSource, _, badBuildEditor) = NewEditor();
var beforeInvalidBuild = Room.CreatedRooms.Count;
var buildFailed = false;
try { _ = badBuildEditor.Execute(new Argument("build " + invalidNumeric)); }
catch (Exception error) when (error.Message == "Unknown direction for reverse")
{
    buildFailed = true;
}
Check(buildFailed, "unsupported numeric build direction throws");
Check(Room.CreatedRooms.Count == beforeInvalidBuild + 1,
    "invalid numeric build allocates a transient room before failing");
Check(badBuildSource.Exits.Count == 0 &&
      Room.CreatedRooms[^1].Exits.Count == 0,
    "invalid numeric build does not publish either exit mapping");

// Editor-style deletion is asymmetric: deleting the outward link leaves
// its counterpart in the destination until separately cleaned up.
var (deleteSource, deleteTarget, deleteEditor) = NewEditor();
Check(!deleteEditor.Execute(new Argument("add East target")),
    "initial two-way add for delete characterization");
Check(deleteEditor.Execute(new Argument("delete East")), "delete handles outgoing link");
Check(deleteSource.Exits.Count == 0 &&
      deleteTarget.Exits.ContainsKey(ArtheaEngine.Model.Direction.West),
    "deleting a two-way link leaves a dangling reverse mapping in target");

Console.WriteLine("Original exit editor one-way and two-way link characterization passed.");
