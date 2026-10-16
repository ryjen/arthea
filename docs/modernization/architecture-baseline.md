# Arthea — legacy dependency inventory and core migration seam

## Observed legacy structure

| Project | Responsibility | Current dependency / risk | Modernization boundary |
| --- | --- | --- | --- |
| ArtheaEngine | Rooms, exits, characters, combat, commands, scripting, persistence | Model classes reference MongoDB.Bson, static DbManager, Jint, NLog and connections; not a pure portable core | Extract and characterize domain behavior before swapping storage |
| ArtheaServer | Telnet sessions, command dispatch, game lifecycle, GUI | Blocking socket accept, thread-per-connection, plaintext credential flow, WinForms | Transport/session adapter, independent of domain |
| ArtheaEditor | Forms and ROM importer | WinForms, legacy driver, eager DB lookups | Preserve and isolate until a separately tested port |
| ArtheaWeb | ASP.NET Web Forms | Legacy .NET Framework web config; insecure public defaults | Do not deploy; separate future decision |
| ArtheaEngine.Tests | NUnit tests | Vendored NUnit and legacy build prerequisites | Characterize semantics before changing them |
| ThirdParty | Vendored source and binaries | Redistribution/provenance not fully known | License inventory and replacement #5 |

## Existing contracts that must survive

- `Room.Exits` is keyed by six `Direction` values: North, South, East, West, Up, Down (underlying numeric 0..5).
- `Exit.ToRoom` and `RoomRef` use database-identified rooms (opaque string IDs).
- The graph can have one-way and vertical exits; do not assume a geometric grid or reverse exit.
- Characters belong to rooms; room chat, command parsing and gameplay semantics must receive **separate characterization tests** before any replacement.
- `Account` credentials and connection handles do not belong in portable core entities.

## First isolated slice

`src/Arthea.Core` is a dependency-free **candidate read-only room-graph seam**, not a full legacy-engine migration. It uses opaque string room IDs, copies mutable inputs, validates dangling references, supports six original directions and one-way exits, and has no network listener, database, scripting or secrets.

`tests/Arthea.Core.SmokeTests` is a minimal executable contract check with **no NuGet test-framework dependencies**. It covers graph navigation, vertical / one-way exits, invalid references, duplicate IDs, and immutable snapshot semantics. It does not prove parity for character or movement side effects.

**Verified validation entrypoint (supported .NET 10 SDK + Python 3):**

    bash scripts/verify-modern-core.sh

The repository pins .NET SDK 10.0.401 with latest-patch roll-forward in `global.json`. The script performs isolated no-external-package restores, warning-as-error builds, executable room-graph tests, *directly compiled original* `ArtheaEngine.Argument` tokenization characterization tests, and Python configuration tests. It does not exercise the original .NET Framework server.

The initial .NET 10 project compiled and its smoke executable ran successfully on the exact PR #8 head `736e7cc80cf35aad5615eb8f792b8e93eb7e4f59` using a SHA-512-verified Microsoft SDK installed in a disposable workspace with Nix ELF-loader adaptation (zero C# build warnings or errors). **The new verification script and added cases require re-validation at the next commit.**

## Legacy parser characterization scope

`tests/Arthea.Legacy.Characterization` compiles the historic `ArtheaEngine/Argument.cs` **from its original path** with a test-only fail-closed shim for the unrelated `IsNumber` string extension (whose containing original utility module depends on the full legacy world/DB graph). It exercises token consumption, peek, custom delimiters and **observed quirks**: a closing quote/parenthesis is retained in the returned token, leading whitespace produces an empty first token, and an unmatched opening quote is emitted separately. These results were discovered by running the original source and are preserved as characterization evidence—not as desired parser behavior. This documents behavior; it does **not** endorse that edge case as safe input handling. It does not characterize `Argument.IsNumber` or the full Telnet command interpreter.

## Original exit-editor behavior — isolated test slice (October 2026)

`tests/Arthea.Legacy.ExitCharacterization` directly compiles the original
`ArtheaServer/Creation/ExitsEditor.cs` and `ArtheaEngine/Argument.cs`
with small, explicitly synthetic models, editor/transport stubs and a
deterministic in-memory room lookup. It is **not** the original .NET Framework
game, MongoDB driver, an authenticated player, or a live editor session.

The tested **`link`** action creates an outgoing exit keyed by the
user-supplied direction and does **not** synthesize a return edge. It leaves
the separate `Exit.Direction` property unset (default `North` in the
historical enum); a linked `West` exit can therefore contain contradictory
metadata. The modern `LegacyRoomProjection` interprets the dictionary key
as authoritative, and its actual implementation is exercised by this suite.

The original `Enum.TryParse` also accepts a numeric direction such as
`99` even though it is not one of the six defined directions. The original
editor can insert it when a synthetic lookup succeeds; the **modern bridge
rejects** numeric or unknown keys. Tests cover mixed-case direction names,
duplicate keys, missing destinations, deleting an outgoing link, and ignoring
an already-absent direction. These are *historical characterizations*, not
desirable input-handling requirements.

**Not yet characterized:** `add`/`build` two-way editing, historic
`Direction.Reverse`, the real database lookup/serialization code, loaded
legacy worlds, travel commands, player `Room.Characters` lifecycle and
multi-user movement side effects. The test-only reverse extension is
intentionally fail-closed to prevent those untested paths from looking
qualified. Original legacy source and protected `master` are untouched.
Existing new-runtime movement/replay tests do **not** establish parity with
the old server. No public deployment or data import is authorized.

## Legacy room projection boundary

The original `Room.Exits` is keyed by `Direction`, while each `Exit` also has a `Direction` field and destination object/`ToRoomRef`. In `ArtheaServer/Creation/ExitsEditor.cs` the **`link`** path adds an exit to the dictionary without assigning `Exit.Direction`; the **`build`** and **`add`** paths set it. Therefore migration reads the dictionary key as the authoritative travel direction and does not infer reverse edges unless they actually exist.

`src/Arthea.LegacyBridge` provides a **database-neutral projection contract**, accepting already-extracted `LegacyRoomSnapshot` records. It validates canonical direction names, destinations and duplicate keys and delegates graph validation (including dangling IDs) to `Arthea.Core`. The projection makes **no MongoDB network calls**, silently-created exits, or database repairs. It does **not** parse raw historic BSON documents or prove compatibility with real stored worlds: capture representative legacy BSON records and implement/test a separate deserialization adapter before migrating data.

## Binary BSON import prototype

`src/Arthea.LegacyBson` now contains a read-only, **bounded** binary BSON decoder using official `MongoDB.Bson` 3.12.0 (Apache-2.0), isolated from both `Arthea.Core` and the source-model projection. The importer consumes individual BSON-encoded room documents; it does **not** connect to MongoDB. It restricts document size and total bytes/rooms, accepts only string `_id`, string `Name`, an `Exits` BSON document keyed by the six named directions, and nested `ToRoomRef` strings; invalid input is rejected without echoing source values. Unrelated legacy fields are ignored rather than executed or materialized as legacy model objects.

`tests/Arthea.LegacyBson.Tests` generates **synthetic binary BSON** with the supported MongoDB.Bson writer, reads it through the importer, and checks navigation, negative inputs, direction-key precedence, one-way exits, and boundary rejection. A synthetic roundtrip does **not establish wire-level compatibility** with the actual historical MongoDB driver/production documents. **Deferred by product decision:** do not collect or verify an authentic historical export for the initial release. The importer is an optional offline experiment and is **not an approved migration path**. If historical save import is ever requested, verify actual legacy encoding and use private sanitized data before enabling it; never commit real exports.

The package-dependent BSON reader and its executable tests carry checked-in `packages.lock.json` files. The verification script uses NuGet `--locked-mode` for that project graph. Changes to MongoDB.Bson or its transitive dependencies require an explicit lockfile update and review; no NuGet packages are needed by the core, legacy parser characterization, or room projection modules.

## Decision: no legacy data migration for the first playable release

The first modern world will be created from fresh, non-sensitive room definitions. The optional BSON reader is retained with its existing synthetic regression tests but will not be connected to startup, deployment, or account migration. **Skipping authenticity verification is an accepted limitation, not evidence of compatibility.** Do not silently attempt to import old data or describe the importer as production-ready. This deferral does not relax the independent historic Git-secret and third-party license reviews required to make the original repository public (#5).

## Next migration steps

1. Keep the pinned .NET 10 SDK and all checks repeatable on an ordinary supported Linux/macOS/Windows CI runner; a validated local Nix-specific wrapper is not yet repo-integrated.
2. Capture *legacy* command/room/movement semantics with runnable fixtures; identify mismatches against this seam.
3. **Start with a new, synthetic world** and a modern authoritative room/chat runtime; do not block initial modernization on authentic legacy-database migration. The BSON prototype remains unsupported for old-world imports until compatibility is separately tested.
4. Implement isolated in-memory, room-scoped social chat and movement with explicit authorization and replay contracts (#3); later add protocol adapters (#1) behind secure transport.
5. Keep PR #6's credential cleanup and #5's historical secret/license gate independent of portable-core completion.

### Known restrictions

- New graph currently rejects dangling destinations; historical Mongo data may be incomplete. This is **not silently backwards compatible** until migration mapping/repair policy is defined.
- Only legacy six directions are represented. Arbitrary portal/special exits are deferred to a versioned extension.
- No core-to-legacy engine runtime integration, binary release, CI or actual player connection exists yet.
