using Arthea.Core.World;

namespace Arthea.Core.Social;

/// <summary>One room-local public message. Sequence numbers are monotonic per room.</summary>
public sealed record RoomChatMessage(long Sequence, string RoomId, string SenderId, string Text);

public sealed record RoomReplay(bool HasGap, IReadOnlyList<RoomChatMessage> Messages);

/// <summary>
/// Bounded, synchronous in-memory proof of room-local social semantics for a fresh world.
/// The caller MUST authenticate player IDs and serialize identities to trusted sessions.
/// No network listeners, persistence, direct messages, moderation, or AI context are present.
/// </summary>
public sealed class RoomChatRuntime
{
    private sealed record Presence(string RoomId, long JoinedAfter);
    private readonly RoomGraph _world;
    private readonly Dictionary<string, Presence> _members = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<RoomChatMessage>> _messages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _latest = new(StringComparer.Ordinal);
    private readonly object _gate = new();
    private readonly int _retention;

    public RoomChatRuntime(RoomGraph world, int retainedPerRoom = 100)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        if (retainedPerRoom is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(retainedPerRoom));
        _retention = retainedPerRoom;
    }

    public void Enter(string playerId, string roomId)
    {
        ValidateIdentity(playerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomId);
        lock (_gate)
        {
            if (!_world.TryGetRoom(roomId, out _))
                throw new ArgumentException("Unknown room.", nameof(roomId));
            if (_members.ContainsKey(playerId))
                throw new InvalidOperationException("Player is already present; use movement or a trusted session resume.");
            _members.Add(playerId, new Presence(roomId, LastSequence(roomId)));
        }
    }

    public bool TryMove(string playerId, Direction direction)
    {
        ValidateIdentity(playerId);
        lock (_gate)
        {
            var member = Member(playerId);
            if (!_world.TryMove(member.RoomId, direction, out var destination) || destination is null)
                return false;
            _members[playerId] = new Presence(destination, LastSequence(destination));
            return true;
        }
    }

    public void Leave(string playerId)
    {
        ValidateIdentity(playerId);
        lock (_gate)
        {
            if (!_members.Remove(playerId))
                throw new InvalidOperationException("Player is not present.");
        }
    }

    public RoomDefinition DescribeRoom(string playerId)
    {
        ValidateIdentity(playerId);
        lock (_gate)
        {
            var id = Member(playerId).RoomId;
            if (!_world.TryGetRoom(id, out var room) || room is null)
                throw new InvalidOperationException("Player room unavailable.");
            return room;
        }
    }

    public string CurrentRoom(string playerId)
    {
        ValidateIdentity(playerId);
        lock (_gate) return Member(playerId).RoomId;
    }

    public RoomChatMessage Say(string playerId, string text)
    {
        ValidateIdentity(playerId);
        if (string.IsNullOrWhiteSpace(text) || text.Length > 512 ||
            text.Any(char.IsControl))
            throw new ArgumentException("Message must be 1-512 characters, with no control characters.", nameof(text));

        lock (_gate)
        {
            var roomId = Member(playerId).RoomId;
            var sequence = checked(LastSequence(roomId) + 1);
            var message = new RoomChatMessage(sequence, roomId, playerId, text);
            if (!_messages.TryGetValue(roomId, out var history))
                _messages[roomId] = history = new List<RoomChatMessage>();
            history.Add(message);
            if (history.Count > _retention)
                history.RemoveAt(0);
            _latest[roomId] = sequence;
            return message;
        }
    }

    /// <summary>
    /// Reads only the current room since this player's last entry into it.
    /// Session authentication, offline identity continuity and durable replay are future work.
    /// </summary>
    public RoomReplay Replay(string playerId, long afterSequence, int maxCount = 50)
    {
        ValidateIdentity(playerId);
        if (afterSequence < 0) throw new ArgumentOutOfRangeException(nameof(afterSequence));
        if (maxCount is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(maxCount));
        lock (_gate)
        {
            var member = Member(playerId);
            if (!_messages.TryGetValue(member.RoomId, out var history) || history.Count == 0)
                return new RoomReplay(false, Array.Empty<RoomChatMessage>());
            var cursor = Math.Max(afterSequence, member.JoinedAfter);
            var gap = history[0].Sequence - 1 > cursor; // Avoid overflow at long.MaxValue.
            var visible = history.Where(m => m.Sequence > cursor).Take(maxCount).ToArray();
            return new RoomReplay(gap, Array.AsReadOnly(visible));
        }
    }

    private long LastSequence(string roomId) =>
        _latest.TryGetValue(roomId, out var sequence) ? sequence : 0;

    private Presence Member(string playerId) =>
        _members.TryGetValue(playerId, out var p) ? p :
            throw new InvalidOperationException("Player is not present.");

    private static void ValidateIdentity(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId) || playerId.Length > 128 ||
            playerId.Any(char.IsControl))
            throw new ArgumentException("Invalid player ID.", nameof(playerId));
    }
}
