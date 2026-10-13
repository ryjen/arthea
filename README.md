# Arthea

**An old C# MUD and personal programming experiment.** Arthea began as a text-based multiplayer game built around rooms, commands, and a server-side world. Most of the effort ended up in the engine and supporting systems rather than game content. It grew much larger than a small text game needed; that is part of its history.

The name **Arthea** is a Pig Latin take on **Earth**.

This repository is primarily a **legacy code archive**, not an actively maintained game, a production service, or a project with a release roadmap.

## Branches

- **[`master`](../../tree/master)** — the original October 2015 code, preserved at commit `62bf4162b686fd1db64a43cc495e7e81868d1679`. No upgrades should be merged here.
- **[`main`](../../tree/main)** — the original project history plus occasional .NET 10 modernization experiments, tests, and small improvements. All new work belongs here.

The code on `main` is **still legacy work**. Modernization is a way to revisit old design decisions, practice architecture and testing, and explore safer implementations. It is **not an attempt to turn Arthea into a current game product**. Experiments may remain unfinished or disconnected from the original application. See the [branch policy](docs/modernization/branch-policy.md).

## What is here

The original application uses .NET Framework, MongoDB, Telnet, and a room-based world model. The repository includes old server, engine, editor, web, and third-party dependency projects.

The newer `src/`, `tests/`, and `samples/` directories contain isolated .NET 10 exercises around room state, text chat, sessions, TLS, GMCP, and an optional SQLite room-message journal. Tests cover selected behaviours; they do not turn the historical application into a complete or supported game.

To exercise the modern test projects on a compatible development system with .NET 10 and Python 3:

```bash
bash scripts/verify-modern-core.sh
```

The original Visual Studio 2010-era solutions and vendored libraries are retained for historical context and are not qualified as a current .NET build. The local demonstration server is synthetic and loopback-only.

## Security and provenance

**Do not deploy the original server or make the repository public as-is.** Legacy code handles passwords insecurely and includes a plaintext Telnet login path. Historical Git commits contain database credentials and potentially sensitive deleted data; removing secrets from current files does not clear history. Third-party dependency licensing also needs review.

The modern experiments are not production-hardening guarantees. Before any publication or public network use, the historical secrets, licenses, authentication, transport and operational risks need independent review. Current configuration and audit tools cover only their documented scope.

See [publication readiness](docs/modernization/publication-readiness.md) and the [issue tracker](https://github.com/ryjen/arthea/issues) for technical notes and unfinished experiments. Issues are **notes and possible exercises, not delivery commitments**.
