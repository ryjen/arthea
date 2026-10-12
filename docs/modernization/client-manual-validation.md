# First desktop client acceptance — local sample

This is a **non-production, synthetic, loopback-only** manual interoperability fixture. It is explicitly launched by a human, cannot listen beyond 127.0.0.1, generates a short-lived self-signed TLS certificate, creates two disposable test-only accounts and a three-room world, and retains all chat and session state in memory. It is not a released server or evidence of security/publication approval.

## Start and inspect

From a system with a supported .NET 10 SDK:

```bash
dotnet run --project samples/Arthea.LocalDemo/Arthea.LocalDemo.csproj -- --self-check
dotnet run --project samples/Arthea.LocalDemo/Arthea.LocalDemo.csproj -- --local-demo
```

Note the randomly assigned port and displayed SHA-256 certificate fingerprint. TLS and server identity verification are required for meaningful client acceptance. Any explicit *test-only* self-signed trust exception must be limited to localhost and confirmed against this fingerprint. Do not disable certificate verification globally.

## Mudlet desktop

Create a new Mudlet profile targeting `127.0.0.1` and the displayed ephemeral port, with **Secure (TLS)** enabled. The sample certificate is ephemeral and self-signed: if the client cannot verify it through an explicit local-only trust mechanism, record a test blocker rather than switching off all certificate checks. After connecting, send `LOGIN alice`, followed by `PASS arthea-demo-alice-ONLY-2026!`. These are **literal synthetic fixture credentials**, never existing user credentials. The server's proof greeting and `SESSION` response are not yet a polished Mudlet login prompt and the bearer token remains visible in the terminal history.

Send `look`, `north`, `south`, `say hello`, `poll 0` and `quit` (command verbs are case-insensitive and chat message case is preserved). Enable GMCP and inspect `gmcp.Room.Info` via the Mudlet debugging UI or a trusted console script. Expect `num` and `id` to be the same opaque room ID, with `name` and direction-to-target `exits`. Do not expect numeric-only IRE mapping scripts to work yet.

## MudForge desktop

Use the desktop client's documented TLS connection flow, with the same loopback address and generated port, and inspect raw GMCP messages using `gmcpdebug`. The default mapper should recognize `Room.Info.num` (string room IDs). Confirm both normal chat and room changes before concluding mapper integration works.

**The iPhone cannot use `127.0.0.1` to reach a different machine.** The local-only demo is intentionally not exposed to a phone or the Internet. Mobile acceptance will require a separately approved TLS/authenticated ingress and end-to-end privacy review, not a temporary public bind.

## Record findings

Capture the client name/version/OS, exact server commit, whether TLS identity verification passed, whether `WILL/DO GMCP` completed, text output, `Room.Info.num` interpretation, one-way/vertical room behavior, missing terminal option/prompt UX, and reconnect expectations. **Never paste passwords, session bearer tokens, certificates with private keys, network captures containing credentials, or private chat into GitHub issues.**

## Known limitations

No user registration, real password migration, durable sessions, account recovery, restart replay, moderation, GMCP terminal ECHO/SGA/NAWS, full Telnet support, browser bridge, mobile routing, production certificate chain, CI, or independent review. The test intentionally remains unmerged and private pending other security gates.
