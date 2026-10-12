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

Run with .NET 10 SDK:

    dotnet build src/Arthea.Core/Arthea.Core.csproj --no-restore
    dotnet run --project tests/Arthea.Core.SmokeTests/Arthea.Core.SmokeTests.csproj

On a fresh machine, restore first (`dotnet restore`), then build. This environment has **not yet executed a C# compiler**. Do not claim these tests have passed until exact-head C# build and execution evidence exists.

## Next migration steps

1. Obtain and pin a supported .NET 10 SDK/toolchain; run compile and smoke tests on exact head.
2. Capture *legacy* command/room/movement semantics with runnable fixtures; identify mismatches against this seam.
3. Add an explicit adapter from legacy persistence room records to the new read model; preserve IDs and fail safely on missing exits.
4. Only then incrementally migrate room chat and world state behind tested interfaces (#3), and eventually protocol adapters (#1).
5. Keep PR #6's credential cleanup and #5's historical secret/license gate independent of portable-core completion.

### Known restrictions

- New graph currently rejects dangling destinations; historical Mongo data may be incomplete. This is **not silently backwards compatible** until migration mapping/repair policy is defined.
- Only legacy six directions are represented. Arbitrary portal/special exits are deferred to a versioned extension.
- No core-to-legacy engine runtime integration, binary release, CI or actual player connection exists yet.
