# Arthea

Arthea is a C# multi-user dungeon (MUD), originally implemented using .NET Framework, MongoDB, Telnet and a room-based world model.

> **Legacy project undergoing modernization. Keep this repository private and do not host the original server for Internet users.** Historical Git commits contain database credentials, and the legacy login transmits and stores raw password bytes. Cleaning HEAD does not clean Git history.

## Modernization goals

- Preserve the original repository, game engine and room graph while incrementally improving architecture and tests.
- Make Arthea a **mobile-first social MUD** centered on human room chat, presence and reconnectable sessions.
- Retain client neutrality: Telnet, Mudlet desktop, and possible MudForge iOS via optional GMCP.
- Introduce safe authentication and TLS before Internet deployment; keep AI NPCs explicitly identified and bounded.
- Audit credentials throughout Git history and the redistribution rights of third-party assets before making source public.

See [publication readiness](docs/modernization/publication-readiness.md) and [project issues](https://github.com/ryjen/arthea/issues).

## Legacy configuration

Existing solutions target Visual Studio 2010-era .NET Framework and historical vendored DLLs; a modern .NET build is not yet verified.

DbManager now requires a non-empty ARTHEA_MONGODB_URI process environment variable with a database name; no credentials or fallback database are tracked in current application configs.

Example for an isolated local-only development instance: set ARTHEA_MONGODB_URI to mongodb://127.0.0.1:27017/arthea-dev (use a separate disposable database for tests). Never commit real connection values or expose this server publicly.

This configuration hygiene work **does not** fix legacy password handling, encrypted transport, ASP.NET exposure, third-party licensing, or historical secrets.

## Original dependencies

The repository retains legacy Jint, NUnit, MongoDB driver and Thought.Net Telnet/Terminals code and binaries pending a license/provenance inventory. Do not assume they are cleared for redistribution.

## Tracking

- [#5 Security/publication gate](https://github.com/ryjen/arthea/issues/5)
- [#3 Room chat, presence and replay](https://github.com/ryjen/arthea/issues/3)
- [#1 Telnet/GMCP interoperability](https://github.com/ryjen/arthea/issues/1)
- [#2 MudForge iPhone testing](https://github.com/ryjen/arthea/issues/2)
- [#4 Conversational AI NPC](https://github.com/ryjen/arthea/issues/4)
