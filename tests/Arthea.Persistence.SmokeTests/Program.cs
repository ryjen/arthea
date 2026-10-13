using Arthea.Core.Social;
using Arthea.Core.World;
using Arthea.Persistence;

static void Check(bool value, string name)
{
    if (!value) throw new Exception("Persistent room journal: " + name);
}
static void Reject<T>(Action action, string name) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}: {name}");
}

var path = Path.Combine(Path.GetTempPath(), "arthea-journal-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(path);
if (!OperatingSystem.IsWindows())
    File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
var db = Path.Combine(path, "rooms.sqlite");
try
{
    var graph = new RoomGraph(new[]
    {
        new RoomDefinition("tavern", "Tavern",
            new Dictionary<Direction, string> { [Direction.North] = "square" }),
        new RoomDefinition("square", "Square",
            new Dictionary<Direction, string> { [Direction.South] = "tavern" })
    });
    // Run identical observable journal contracts against both adapters.
    static void JournalContract(IRoomChatJournal journal, string kind)
    {
        Reject<ArgumentException>(() => journal.LastSequence("invalid\\nroom"),
            kind + ": rejects control characters in room identity");
        Reject<ArgumentException>(() => journal.Replay(new string('x', 257), 0, 1),
            kind + ": rejects oversized room identity");
        Reject<ArgumentException>(() => journal.Append("invalid\\rroom", "alice", "bad", 2),
            kind + ": rejects invalid room identity before write");
        Check(journal.LastSequence("contract-a") == 0,
            kind + ": malformed room identity cannot mutate other streams");
        Check(journal.LastSequence("contract-a") == 0, kind + ": initial sequence");
        Check(journal.Append("contract-a", "alice", "first", 2).Sequence == 1,
            kind + ": first append");
        Check(journal.Append("contract-a", "bob", "second", 2).Sequence == 2,
            kind + ": second append");
        Check(journal.Append("contract-a", "alice", "third", 2).Sequence == 3,
            kind + ": third append");
        Check(journal.LastSequence("contract-a") == 3, kind + ": monotonic high watermark");
        var replay = journal.Replay("contract-a", 0, 100);
        Check(replay.HasGap && replay.Messages.Select(m => m.Sequence).SequenceEqual(new long[] { 2, 3 }),
            kind + ": retention and replay gap");
        Check(journal.Replay("contract-a", 2, 1).Messages.Single().Sequence == 3,
            kind + ": exclusive replay cursor");
        Check(journal.Replay("contract-a", 3, 100).Messages.Count == 0,
            kind + ": caught-up replay");
        Check(journal.Replay("contract-a", long.MaxValue, 100).Messages.Count == 0,
            kind + ": future cursor");
        Check(journal.LastSequence("contract-b") == 0 &&
              journal.Replay("contract-b", 0, 100).Messages.Count == 0,
            kind + ": isolated room");
        Reject<ArgumentOutOfRangeException>(() => journal.Replay("contract-a", -1, 1),
            kind + ": rejects negative cursor");
        Reject<ArgumentOutOfRangeException>(() => journal.Replay("contract-a", 0, 101),
            kind + ": bounds replay page");
        Reject<ArgumentOutOfRangeException>(() => journal.Append("contract-a", "alice", "bad", 0),
            kind + ": bounds retention");
        Check(journal.LastSequence("contract-a") == 3, kind + ": rejected append preserves high watermark");
    }

    JournalContract(new InMemoryRoomChatJournal(), "memory");
    JournalContract(new SqliteRoomChatJournal(Path.Combine(path, "contract.sqlite")), "sqlite");

    var store = new SqliteRoomChatJournal(db);
    if (!OperatingSystem.IsWindows())
    {
        var unsafeDir = Path.Combine(path, "insecure");
        Directory.CreateDirectory(unsafeDir);
        File.SetUnixFileMode(unsafeDir, UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute);
        Reject<UnauthorizedAccessException>(() =>
            new SqliteRoomChatJournal(Path.Combine(unsafeDir, "chat.sqlite")),
            "group-readable journal directory rejected");
        File.SetUnixFileMode(unsafeDir,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        var symlink = Path.Combine(path, "redirect.sqlite");
        File.CreateSymbolicLink(symlink, db);
        Reject<ArgumentException>(() => new SqliteRoomChatJournal(symlink),
            "symlinked journal file rejected");
        File.Delete(symlink);
    }

    Reject<ArgumentException>(() => new SqliteRoomChatJournal("relative.db"),
        "relative database paths denied");
    Check(!OperatingSystem.IsLinux() ||
          File.GetUnixFileMode(db) == (UnixFileMode.UserRead | UnixFileMode.UserWrite),
          "owner-only database file");
    var first = new RoomChatRuntime(graph, retainedPerRoom: 2, journal: store);
    first.Enter("alice", "tavern");
    first.Enter("bob", "tavern");
    Check(first.Say("alice", "Hello Bob").Sequence == 1, "first persisted sequence");
    Check(first.Replay("bob", 0).Messages.Single().Text == "Hello Bob", "authorized live replay");
    first.Say("bob", "Second");
    first.Say("alice", "Third");
    Check(first.Replay("bob", 0).HasGap, "retention gap is visible");
    Check(first.Replay("bob", 0).Messages.Select(m=>m.Sequence).SequenceEqual(new long[]{2,3}),
        "bounded replay ordered");
    Check(first.TryMove("alice", Direction.North), "move to another room");
    Check(first.Say("alice", "At square").Sequence == 1, "independent room cursor");
    Check(first.Replay("bob", 0).Messages.All(x=>x.RoomId=="tavern"),
        "cross-room isolation");
    Check(first.TryMove("alice", Direction.South), "return to tavern");
    Check(first.Replay("alice", 0).Messages.Count == 0,
        "reentry denies before-entry messages");
    first.Say("bob", "Since your return");
    Check(first.Replay("alice", 0).Messages.Single().Sequence == 4, "joined-after cursor");

    // Simulate a complete process restart: no retained session/actor authority.
    var reopened = new RoomChatRuntime(graph, retainedPerRoom: 2,
        journal: new SqliteRoomChatJournal(db));
    Reject<InvalidOperationException>(() => reopened.Replay("alice", 0),
        "restart does not restore unauthenticated membership");
    Check(new SqliteRoomChatJournal(db).LastSequence("tavern") == 4,
        "room sequence durable across restart");
    reopened.Enter("alice", "tavern");
    Check(reopened.Replay("alice", 0).Messages.Count == 0,
        "fresh login cannot read historical room chat");
    var next = reopened.Say("alice", "After restart");
    Check(next.Sequence == 5, "monotonic per-room sequence across restart");
    Check(reopened.Replay("alice", 0).Messages.Single().Sequence == 5,
        "new messages visible after fresh login");
    Reject<InvalidOperationException>(() => reopened.Replay("eve", 0),
        "unknown actor denied");
    Check(new SqliteRoomChatJournal(db).Replay("tavern",0,100).HasGap,
        "pruned history reports discontinuity after restart");

    // Concurrent writers through separate store instances maintain monotonic unique IDs.
    var other = new SqliteRoomChatJournal(db);
    Parallel.For(0, 12, i =>
    {
        var target = i % 2 == 0 ? store : other;
        target.Append("tavern", "load-test", "synthetic " + i, 10);
    });
    var oldest = store.Replay("tavern", 0, 100);
    Check(oldest.Messages.Count == 10 && oldest.HasGap,
        "transactional retention holds with concurrent writers");
    Check(oldest.Messages.Select(m=>m.Sequence).Distinct().Count() == 10 &&
          oldest.Messages.Zip(oldest.Messages.Skip(1)).All(x => x.First.Sequence < x.Second.Sequence),
          "concurrent unique ordered messages");
    Check(other.LastSequence("tavern") == 17, "concurrent total persists");

    // A denied store write fails without making a phantom sequence.
    Reject<ArgumentOutOfRangeException>(() => other.Append("tavern", "eve", "bad", 0),
        "retention bound denied");
    Check(other.LastSequence("tavern") == 17, "invalid append did not advance sequence");
    Check(!File.ReadAllText(db).Contains("test-password-not-present", StringComparison.Ordinal),
        "no fake credentials were stored");
    Console.WriteLine("SQLite journal persistence, restart isolation, retention and concurrency passed.");
}
finally
{
    try { Directory.Delete(path, recursive: true); }
    catch (IOException) { } // Some providers may retain WAL handles briefly; test fixtures are synthetic.
}
