namespace Arthea.Core.Social;

/// <summary>
/// Server-side append-only room-journal abstraction. Callers enforce room
/// membership before invoking it. No public API for arbitrary clients.
/// </summary>
public interface IRoomChatJournal
{
    long LastSequence(string roomId);
    RoomChatMessage Append(string roomId, string senderId, string text, int retainedPerRoom);
    RoomReplay Replay(string roomId, long afterSequence, int maxCount);
}
