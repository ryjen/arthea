# Arthea experimental session ownership

The `RoomSessionCoordinator` in `Arthea.Core.Social` is a single-process **contract prototype** around the fresh-world `RoomChatRuntime`. It is **not a production identity system**.

## Trust boundary

- `IPlayerAuthenticator.Authenticate(credential)` is an **injected external verifier**. The only implementation shipped is a synthetic test fake; there is no real login, password verification, authorization database or network listener. Never accept a player ID supplied by a remote client as proof of identity.
- The authenticator must be implemented and reviewed separately with TLS, abuse/rate limiting and secure credential storage. Authentication is performed outside the room/session state lock.
- On successful verification, the coordinator mints a random 256-bit session bearer token. Only SHA-256 digests are stored in its in-memory session table. The raw token is returned once to the trusted client boundary. Never log, include in URLs, place in telemetry or persist it in plaintext.
- Session admission is capped; one player may have at most one nonexpired session. Implicitly replacing sessions via a second login is prohibited; disconnect followed by `Resume` rotates the token and invalidates the previous token.
- `Suspend` prevents gameplay calls while the transport is disconnected, but retains room membership and a bounded public-room replay window until expiration. `Resume` requires possession of the prior bearer; it does not prove a real mobile device or account identity independently.
- The session expiry is absolute and validated on every operation. An explicit `ExpireSessions` provides cleanup when idle; expired sessions have their membership revoked. **No background timer or persistent session storage exists.**
- The `RoomChatRuntime` remains directly callable by trusted server code. Production must encapsulate both within an authenticated server boundary; do not expose its player-ID methods to remote clients.

## Deliberate deferrals

The system cannot yet serve remote players. Missing: real authenticator and credential lifecycle, TLS, secure ingress/identity binding, token replay prevention across a disconnected/stolen device, durable offline chat, cross-device revocation, password reset, room permissions, DMs/channels, rate limits, moderation, telemetry redaction, storage, and protocol interoperability. Tokens are per-process only and cannot survive restart. A reconnect after token expiry requires fresh authentication and rejoining without historic replay.

No confidential or real credentials, live data, database account migrations, or Internet endpoints are used in these tests.

## Verification

`tests/Arthea.Session.SmokeTests` exercises identity forgery rejection, capacity, duplicate sessions, suspend/resume, token rotation and old-token invalidation, expired token cleanup, replay after disconnect, room movement, fresh-session replay restrictions and a concurrent resume race. Run `bash scripts/verify-modern-core.sh` from a supported .NET 10 environment.

Publication remains blocked by the historical secret/licensing audit in #5. A passing test here does not authorize deployment.
