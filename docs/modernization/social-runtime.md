# Arthea social runtime — experimental in-memory contract

The initial Arthea world uses **fresh room definitions**, not a historic database import. `Arthea.Core.Social.RoomChatRuntime` demonstrates server-controlled, room-local, human-to-human chat and directional movement without external dependencies.

This initial test-only vertical slice has: unique player presence in one room, explicit enter/move/leave operations, per-room ordered sequence numbers, bounded message length and history, cursor replay, and a visible gap flag when retention expired. It prevents a participant from reading another room or messages that predate their most recent room entry.

**Security and lifecycle limitations:** Player identifiers passed to this class must come from an independently authenticated trusted session. It does not implement authentication, TLS, ACLs, DMs, moderation, durable message history, disconnect/resume reconciliation, AI/NPC context, persistence, broadcast queues or network transport. The runtime is an in-memory prototype, not a deployable multiplayer server. Its lock is local to a single process and must not be interpreted as a distributed consistency guarantee.

The console test `tests/Arthea.Social.SmokeTests` checks two participants in the same room, an isolated third party, movement, one-way exits, re-entry history privacy, bounded replay, sequence ordering and ANSI/control-input rejection. It does not constitute Internet-security or concurrency performance qualification.

## Follow-up

Integrate an authoritative session and identity boundary, write explicit durable history/retention policy and reconnect semantics, validate moderation and channel isolation, and only then expose this through a reviewed secure transport (#1, #3, #5).
