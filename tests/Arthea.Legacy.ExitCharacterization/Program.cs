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
Check(editor.Execute(new Argument("link 99 destination")), "numeric direction is accepted by editor");
Check(origin.Exits.ContainsKey((ArtheaEngine.Model.Direction)99),
    "invalid numeric direction inserted into legacy dictionary");
var invalidLegacy = new[]
{
    new LegacyRoomSnapshot(origin.Id, origin.Name,
        new Dictionary<string, LegacyExitSnapshot>
        {
            ["99"] = new(destination.Id, null),
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
Check(origin.Exits.ContainsKey((ArtheaEngine.Model.Direction)99),
    "delete does not accidentally clear unrelated links");
Check(destination.Exits.Count == 0, "delete leaves destination unchanged");
Check(editor.Execute(new Argument("delete West")), "missing deletion handled");
Check(origin.Exits.Count == 1, "missing deletion leaves remaining links unchanged");

Console.WriteLine("Original exit editor one-way link characterization passed.");
