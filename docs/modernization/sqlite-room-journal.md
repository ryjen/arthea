# Local SQLite room journal — #3

This is a **private, experimental** persistence adapter for *public room chat only*. The in-memory `Arthea.Core.Social.RoomChatRuntime` remains the owner of player/room membership and permission checks. `Arthea.Persistence.SqliteRoomChatJournal` implements a pure core interface, `IRoomChatJournal`, and is **opt-in** through the room-runtime constructor. The existing local TLS demo continues to use in-memory chat by default.

## Semantics

- Only authorized `Say` calls (authenticated/session-bound in the coordinator) append. A SQLite **immediate write transaction** allocates and commits one monotonic per-room sequence number, message, and per-room retention trim atomically. Deleted/pruned messages never cause sequence numbers to be reused, including after restart.
- The core computes `max(requestedCursor, latestAtRoomEntry)`, so replay is still restricted to the player's current room and to messages after the player's most recent entry. A result reports an explicit `HasGap` when retention has truncated the requested range.
- After process restart, public-room messages and sequence counters survive. **Sessions, bearer tokens, online memberships and replay grants do NOT survive**. Freshly authenticated joins start at the persisted latest sequence and **cannot read old room messages**. This prevents reopening pre-entry history by guessing a cursor; it does not yet implement offline catch-up across server restarts.
- A durable offline replay grant, if needed, requires separately reviewed persistent identity, room membership/entry epochs, revocation and crash-consistent session migration. Avoid persisting raw bearer tokens. Do not enable history reads directly through remote commands.

## Security/operations

The journal takes an absolute file path inside an existing private directory; it does not accept arbitrary SQL from a player. SQL statements parameterize room IDs and message text, and the file is owner-readable/writable only on Unix. The containing directory, SQLite `-wal`/`-shm` files, snapshots and backups must also be owner-private. SQLite content is **not encrypted at rest**: use host disk encryption, backup controls and a written retention/deletion policy before real players. This prototype enforces a count-based per-room retention limit (1–1000 messages) rather than an age-based retention/erasure policy. Do not log or upload raw DB files.

The adapter uses a locked Microsoft.Data.Sqlite dependency and a separate `Arthea.Persistence` project. Neither MongoDB nor SQLite leaks into `Arthea.Core`, and no historical BSON account import is attempted. No production launch, migration, or repository-public permission is implied.

## Remaining work

Durable authorized offline replay across restart; per-user read receipts and deduplication; chat delete/retention policies; private DM/channel isolation; cross-device login and moderation; operational backup/restore/corruption testing; performance & multi-host correctness; secure client credentials; and real Mudlet/MudForge acceptance. #5 and #12 remain independent release gates.
