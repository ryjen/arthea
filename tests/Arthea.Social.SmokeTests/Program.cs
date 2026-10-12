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

Console.WriteLine("Fresh-world room-local chat, movement, isolation and replay smoke tests passed.");
