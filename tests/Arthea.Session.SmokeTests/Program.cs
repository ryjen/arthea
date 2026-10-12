using Arthea.Core.World;
using Arthea.Core.Social;

static void Check(bool result, string label)
{
    if (!result) throw new Exception("Session test failed: " + label);
}
static void Reject<T>(Action action, string label) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}: {label}");
}

var clock = new ManualClock(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
var world = new RoomGraph(new[]
{
    new RoomDefinition("tavern", "Tavern", new Dictionary<Direction, string> { [Direction.North] = "square" }),
    new RoomDefinition("square", "Square", new Dictionary<Direction, string> { [Direction.South] = "tavern" }),
    new RoomDefinition("cellar", "Cellar", new Dictionary<Direction, string>())
});
var chat = new RoomChatRuntime(world, retainedPerRoom: 2);
var service = new RoomSessionCoordinator(chat, new StubAuthenticator(), clock,
    TimeSpan.FromMinutes(5), maxSessions: 2);

Reject<InvalidOperationException>(() => service.Connect("invalid", "tavern"), "bad credential fails closed");
Reject<InvalidOperationException>(() => service.Connect("alice", "missing"), "cannot assert untrusted identity");
var alice = service.Connect("credential-alice", "tavern");
var bob = service.Connect("credential-bob", "tavern");
Check(alice.Token.Length == 64 && alice.Token != bob.Token, "random unique tokens");
Check(!alice.ToString().Contains(alice.Token, StringComparison.Ordinal), "record formatting must not leak the bearer token");
Check(!System.Text.Json.JsonSerializer.Serialize(alice).Contains(alice.Token, StringComparison.Ordinal),
    "default JSON serialization must not disclose bearer tokens");
Reject<InvalidOperationException>(() => service.Connect("credential-alice", "square"), "no duplicate session or teleport");
Reject<InvalidOperationException>(() => service.Connect("credential-eve", "cellar"), "bounded session admission");
Reject<InvalidOperationException>(() => service.Say("alice", "spoof"), "cannot use player ID as bearer token");
Reject<InvalidOperationException>(() => service.Say(new string('a', 64), "spoof"), "unissued token rejected");

service.Say(alice.Token, "hello");
Check(service.Replay(bob.Token, 0).Messages.Single().Text == "hello", "same-room authorized replay");
service.Suspend(alice.Token);
Reject<InvalidOperationException>(() => service.Say(alice.Token, "offline spoof"), "suspended token cannot act");
Reject<InvalidOperationException>(() => service.Replay(alice.Token, 0), "suspended token cannot read");

service.Say(bob.Token, "missed message");
var resumed = service.Resume(alice.Token);
Check(resumed.Token != alice.Token && resumed.PlayerId == alice.PlayerId, "resume rotates token and binds identity");
Reject<InvalidOperationException>(() => service.Resume(alice.Token), "consumed resume token cannot be reused");
Reject<InvalidOperationException>(() => service.Say(alice.Token, "spoof"), "old bearer revoked");
Check(service.Replay(resumed.Token, 1).Messages.Single().Text == "missed message", "replay after mobile reconnect");
Check(service.TryMove(resumed.Token, Direction.North), "authenticated movement");
Check(service.CurrentRoom(resumed.Token) == "square", "room preserved by coordinator");
Check(service.Replay(resumed.Token, 0).Messages.Count == 0, "new room has no prior public replay");

clock.Advance(TimeSpan.FromMinutes(5));
Reject<InvalidOperationException>(() => service.Say(resumed.Token, "expired"), "expiry enforced at exact deadline");
Check(service.ExpireSessions() == 0, "expired sessions pruned during prior call");
Reject<InvalidOperationException>(() => service.Resume(resumed.Token), "expired bearer cannot reconnect");
Reject<InvalidOperationException>(() => service.Replay(bob.Token, 0), "other expired session cannot replay");
var again = service.Connect("credential-alice", "square");
Check(service.Replay(again.Token, 0).Messages.Count == 0, "fresh auth cannot read earlier joined history");
service.Logout(again.Token);
Reject<InvalidOperationException>(() => service.Say(again.Token, "post logout"), "logout revokes bearer");
Check(service.ExpireSessions() == 0, "no orphaned sessions");

var races = new RoomSessionCoordinator(new RoomChatRuntime(world),
    new StubAuthenticator(), clock, TimeSpan.FromMinutes(5));
var initial = races.Connect("credential-bob", "tavern");
races.Suspend(initial.Token);
var successes = 0;
Parallel.For(0, 8, _ =>
{
    try
    {
        races.Resume(initial.Token);
        Interlocked.Increment(ref successes);
    }
    catch (InvalidOperationException) { }
});
Check(successes == 1, "parallel resume has exactly one winner");
Reject<InvalidOperationException>(() => races.Resume(initial.Token), "old token still invalid after race");

Console.WriteLine("Session ownership, expiry, rotation, replay and reconnect contracts passed.");

sealed class StubAuthenticator : IPlayerAuthenticator
{
    public string? Authenticate(string credential) => credential switch
    {
        "credential-alice" => "alice",
        "credential-bob" => "bob",
        "credential-eve" => "eve",
        _ => null
    };
}

sealed class ManualClock(DateTimeOffset initial) : TimeProvider
{
    private DateTimeOffset _now = initial;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan amount) => _now += amount;
}
