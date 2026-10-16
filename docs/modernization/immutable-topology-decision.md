# ADR — Immutable topology before an editor

- **Status:** Accepted for the current .NET 10 archaeology exercise (2026-10-16)
- **Scope:** `Arthea.Core.World.RoomGraph` and its existing consumers
- **Follow-up:** [Issue #7](https://github.com/ryjen/arthea/issues/7); legacy editor security qualification remains in [#12](https://github.com/ryjen/arthea/issues/12)

## Context

Original `ExitsEditor.cs` has been characterized in [PR #51](https://github.com/ryjen/arthea/pull/51) and [PR #52](https://github.com/ryjen/arthea/pull/52). Its `link` action makes directed exits; `add` and `build` can make reciprocal exits, but `add` reports false even after two mutations. A later one-sided `delete` can leave a reverse backlink. Invalid numeric directions may be parsed, then fail during reverse lookup; a failing `build` may have allocated a room before that failure. These are historical observations, **not** accepted behavior for a new editor.

The modern `RoomGraph` is a **read-only, validated and defensively copied snapshot**. Its constructor validates every declared room, direction, destination and endpoint before exposing the new instance; `Rooms` and `TryGetRoom` expose read-only exit dictionaries. `RoomChatRuntime` retains a readonly graph reference, and no modern editor, topology mutation method, or runtime graph-replacement operation currently exists. `TryMove` changes player presence, not the graph.

## Decision

**Do not add an atomic mutable two-way-exit editing API to `RoomGraph`, `RoomChatRuntime` or `RoomSessionCoordinator` without an actual, authorized modern editor consumer.** Preserve one-way, two-way and vertical exits as independently declared directed edges and preserve the historic direction numeric compatibility contract.

The existing atomicity boundary is **constructing a new full `RoomGraph` snapshot**: either validation succeeds and the caller gets a separate graph, or construction throws and any previously held graph remains untouched. This does not publish new topology into an active runtime, persist anything, or make changes atomic across sessions or processes.

The dependency-free contract tests now assert:

1. Explicit reciprocal declarations survive mutation of caller-owned input maps.
2. Exposed room exit dictionaries reject writes after graph publication.
3. An invalid replacement candidate fails without changing an existing graph.

No concurrency, persistence, editor privilege or multi-process transaction guarantee follows from these tests.

## Conditions for reconsidering

If a modern world editor is actually authorized, introduce a separate **offline editor/builder** rather than changing `RoomGraph` into an exposed mutable collection. Specify and independently test its contract before implementation:

- **Explicit intent:** one-way or reciprocal link operations; no automatic reciprocity for unrelated existing edges. Resolve reverse directions with a closed enum mapping; reject unknown/numeric-out-of-range values before allocations or writes.
- **All-or-nothing candidate:** validate both room IDs, existence, direction, conflicts and reverse-direction occupancy before updating *staged copies*. Do not mutate any published `RoomDefinition`, `RoomGraph`, or runtime presence during staging.
- **Consistent removal:** deleting a reciprocal link should verify the expected reverse endpoint and remove both staged edges, or reject the operation without a partial result; never delete an unrelated one-way link.
- **Publishing:** build and validate a whole candidate graph; use explicit version/check-and-swap semantics at a higher **trusted authority boundary**. A running `RoomChatRuntime` currently has no way to replace its readonly graph. A future runtime replacement must coordinate identity, player locations, room deletion, session authorization, replay/journal and concurrent movement, with rollbacks/failure injection. Do not infer that an in-memory snapshot rebuild itself solves these.
- **Durability:** if persistence is introduced, define a separate all-or-nothing storage transaction and restart/recovery behavior; committed bytes and visible live graph must not diverge.
- **Security:** authenticate/authorize editor actions and bound payload sizes. No legacy Telnet editor, database access, public host, or unrestricted command execution is authorized.

Require a new decision and issue tied to a concrete editor requirement and tests before opening this broader implementation. No open-ended engine rewrite, lock redesign, or live topology mutation is justified by legacy code archaeology alone.

## Alternatives considered

| Alternative | Disposition |
| --- | --- |
| Add `SetExit`/`RemoveExit` on the shared runtime graph now | Rejected: introduces shared mutable topology, partial reciprocal edits and session/race complexity without a consumer |
| Blindly port original `ExitsEditor` to modern .NET | Rejected: reproduces demonstrated reporting/atomicity and input bugs, along with dependency entanglement |
| Keep immutable snapshot, optionally stage a new candidate offline later | **Selected:** minimal API surface and fail-closed existing validation, without claiming live atomic updates |

## Verification boundary

Current core tests validate snapshot immutability and rejection of malformed candidates. The original editor tests in `tests/Arthea.Legacy.ExitCharacterization` remain characterization only. A passing CI run verifies these exercises, **not** original .NET Framework functionality, full editor integration, cross-process atomicity, data migration, third-party redistribution rights, or public service readiness.
