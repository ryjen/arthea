using Arthea.Core.Social;
using Arthea.Core.World;

static void Check(bool value, string label)
{
    if (!value) throw new Exception("Social contract failed: " + label);
}
static void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name + ": " + label);
}

var world = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "The Rusty Lantern",
        new Dictionary<Direction, string> { [Direction.North] = "square", [Direction.Down] = "cellar" }),
    new RoomDefinition("square", "Market Square",
        new Dictionary<Direction, string> { [Direction.South] = "tavern" }),
    new RoomDefinition("cellar", "Cellar", new Dictionary<Direction, string>()),
});
var chat = new RoomChatRuntime(world, retainedPerRoom: 2);
chat.Enter("alice", "tavern");
chat.Enter("bob", "tavern");
chat.Enter("eve", "cellar");

var first = chat.Say("alice", "Hello Bob");
Check(first.Sequence == 1 && first.SenderId == "alice" && first.RoomId == "tavern", "first message identity and cursor");
Check(chat.Replay("bob", 0).Messages.Count == 1, "same-room message accessible");
Check(chat.Replay("eve", 0).Messages.Count == 0, "unrelated room messages never exposed");
Reject<InvalidOperationException>(() => chat.Replay("unjoined", 0), "no unauthenticated replay");
Reject<InvalidOperationException>(() => chat.Say("unjoined", "forged"), "no unjoined sender");
Reject<InvalidOperationException>(() => chat.Enter("alice", "cellar"), "join cannot teleport existing participant");

chat.Say("bob", "Welcome");
chat.Say("alice", "What brings you here?");
var replay = chat.Replay("bob", 0);
Check(replay.HasGap && replay.Messages.Count == 2, "bounded retention reports replay gap");
Check(replay.Messages[0].Sequence == 2 && replay.Messages[1].Sequence == 3, "strict per-room order");
Check(chat.Replay("bob", 2).Messages.Single().Sequence == 3, "cursor resumes without duplicates");
Check(!chat.Replay("bob", 3).HasGap && chat.Replay("bob", 3).Messages.Count == 0, "caught-up replay");
Check(!chat.Replay("bob", long.MaxValue).HasGap && chat.Replay("bob", long.MaxValue).Messages.Count == 0, "future cursors cannot overflow");

Check(chat.TryMove("alice", Direction.North), "valid linked movement");
Check(chat.CurrentRoom("alice") == "square", "authoritative room changed");
Check(!chat.TryMove("alice", Direction.Up), "invalid movement rejected");
Check(chat.Say("alice", "Anyone at the square?").Sequence == 1, "room cursors independent");
Check(chat.Replay("bob", 0).Messages.All(m => m.RoomId == "tavern"), "previous room cannot access new room");
Check(chat.TryMove("alice", Direction.South), "return path");
Check(chat.Replay("alice", 0).Messages.Count == 0, "reentering room cannot replay before joining");
chat.Say("bob", "You are back");
Check(chat.Replay("alice", 0).Messages.Single().Text == "You are back", "new chat since reentry visible");

Check(chat.TryMove("eve", Direction.Up) == false, "one-way cellar is not reversed");
chat.Leave("bob");
Reject<InvalidOperationException>(() => chat.Replay("bob", 0), "departed participant cannot read history");
chat.Enter("bob", "tavern");
Check(chat.Replay("bob", 0).Messages.Count == 0, "fresh join cannot read earlier chat");
Reject<ArgumentException>(() => chat.Say("alice", "\u001b[31mspoof"), "ANSI/control injection rejected");
Reject<ArgumentException>(() => chat.Say("alice", new string('x', 513)), "oversized messages rejected");
Reject<ArgumentException>(() => chat.Enter(" ", "tavern"), "blank player identity rejected");
Reject<ArgumentOutOfRangeException>(() => chat.Replay("alice", 0, 101), "replay page bounded");
Reject<ArgumentException>(() => chat.Enter("new", "invalid"), "unknown room rejected");
Reject<ArgumentOutOfRangeException>(() => new RoomChatRuntime(world, 0), "invalid memory budget");

// The default journal and explicit constructor-injected journal share semantics.
var journal = new InMemoryRoomChatJournal();
Check(journal.LastSequence("square") == 0, "empty journal cursor");
Check(journal.Append("square", "alice", "one", 2).Sequence == 1, "journal first sequence");
Check(journal.Append("square", "alice", "two", 2).Sequence == 2, "journal second sequence");
Check(journal.Append("square", "alice", "three", 2).Sequence == 3, "journal retains monotonic cursor");
Check(journal.LastSequence("square") == 3, "journal high watermark survives trim");
var retained = journal.Replay("square", 0, 50);
Check(retained.HasGap && retained.Messages.Select(x => x.Sequence).SequenceEqual(new long[] { 2, 3 }),
    "in-memory journal retention gap and order");
Check(!journal.Replay("cellar", 0, 50).Messages.Any(), "journals isolate room history");
Reject<ArgumentOutOfRangeException>(() => journal.Replay("square", -1, 1), "negative cursor");
Reject<ArgumentOutOfRangeException>(() => journal.Append("square", "alice", "text", 0), "invalid retention");

var injected = new RoomChatRuntime(world, retainedPerRoom: 2, journal: new InMemoryRoomChatJournal());
injected.Enter("alice", "tavern");
injected.Enter("bob", "tavern");
Check(injected.Say("alice", "injected").Sequence == 1 &&
      injected.Replay("bob", 0).Messages.Single().Text == "injected",
    "constructor injected memory journal");

// Journal lookup must complete before any presence transition is published.
var faults = new FaultJournal();
var atomic = new RoomChatRuntime(world, journal: faults);
faults.FailLookup = true;
Reject<InvalidOperationException>(() => atomic.Enter("alice", "tavern"),
    "failed journal lookup prevents enter");
faults.FailLookup = false;
atomic.Enter("alice", "tavern");
Check(atomic.CurrentRoom("alice") == "tavern", "retry enter succeeds after lookup fault");
faults.FailLookup = true;
Reject<InvalidOperationException>(() => atomic.TryMove("alice", Direction.North),
    "failed destination lookup prevents movement");
Check(atomic.CurrentRoom("alice") == "tavern", "failed movement retains original room");
faults.FailLookup = false;
Check(atomic.TryMove("alice", Direction.North), "movement succeeds after transient fault");
Check(atomic.CurrentRoom("alice") == "square", "successful movement publishes destination");
faults.FailAppend = true;
Reject<InvalidOperationException>(() => atomic.Say("alice", "not committed"),
    "append failure propagates without a message");
faults.FailAppend = false;
Check(atomic.Say("alice", "committed").Sequence == 1,
    "failed append does not consume in-memory cursor");
faults.FailReplay = true;
Reject<InvalidOperationException>(() => atomic.Replay("alice", 0),
    "replay failure propagates without altering membership");
faults.FailReplay = false;
Check(atomic.CurrentRoom("alice") == "square" &&
      atomic.Replay("alice", 0).Messages.Single().Text == "committed",
    "replay recovers after transient fault");

Console.WriteLine("Fresh-world room-local chat, movement, isolation and replay smoke tests passed.");

// This deliberately small test double exposes persistence faults without mocking
// session identity or network authentication.
sealed class FaultJournal : IRoomChatJournal
{
    private readonly InMemoryRoomChatJournal _inner = new();
    public bool FailLookup { get; set; }
    public bool FailAppend { get; set; }
    public bool FailReplay { get; set; }

    public long LastSequence(string roomId) =>
        FailLookup ? throw new InvalidOperationException("Injected lookup failure") :
        _inner.LastSequence(roomId);

    public RoomChatMessage Append(string roomId, string senderId, string text, int retainedPerRoom) =>
        FailAppend ? throw new InvalidOperationException("Injected append failure") :
        _inner.Append(roomId, senderId, text, retainedPerRoom);

    public RoomReplay Replay(string roomId, long afterSequence, int maxCount) =>
        FailReplay ? throw new InvalidOperationException("Injected replay failure") :
        _inner.Replay(roomId, afterSequence, maxCount);
}

