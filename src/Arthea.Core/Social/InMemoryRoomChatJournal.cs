namespace Arthea.Core.Social;

/// <summary>
/// Bounded, single-process in-memory room journal. The caller owns authorization
/// and room membership. This adapter has no persistence or identity semantics.
/// </summary>
public sealed class InMemoryRoomChatJournal : IRoomChatJournal
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<RoomChatMessage>> _messages = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _latest = new(StringComparer.Ordinal);

    public long LastSequence(string roomId)
    {
        ValidateRoom(roomId);
        lock (_gate)
            return _latest.TryGetValue(roomId, out var value) ? value : 0;
    }

    public RoomChatMessage Append(string roomId, string senderId, string text, int retainedPerRoom)
    {
        ValidateRoom(roomId);
        ArgumentException.ThrowIfNullOrWhiteSpace(senderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (retainedPerRoom is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(retainedPerRoom));
        lock (_gate)
        {
            var sequence = checked((_latest.TryGetValue(roomId, out var last) ? last : 0) + 1);
            var message = new RoomChatMessage(sequence, roomId, senderId, text);
            if (!_messages.TryGetValue(roomId, out var history))
                _messages[roomId] = history = new List<RoomChatMessage>();
            history.Add(message);
            if (history.Count > retainedPerRoom)
                history.RemoveAt(0);
            _latest[roomId] = sequence;
            return message;
        }
    }

    public RoomReplay Replay(string roomId, long afterSequence, int maxCount)
    {
        ValidateRoom(roomId);
        if (afterSequence < 0) throw new ArgumentOutOfRangeException(nameof(afterSequence));
        if (maxCount is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(maxCount));
        lock (_gate)
        {
            if (!_messages.TryGetValue(roomId, out var history) || history.Count == 0)
                return new RoomReplay(false, Array.Empty<RoomChatMessage>());
            var gap = history[0].Sequence - 1 > afterSequence;
            var visible = history.Where(m => m.Sequence > afterSequence).Take(maxCount).ToArray();
            return new RoomReplay(gap, Array.AsReadOnly(visible));
        }
    }

    private static void ValidateRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            throw new ArgumentException("Invalid room identity.", nameof(roomId));
    }
}
