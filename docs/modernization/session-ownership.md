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

## Real password verifier, synthetic records

The newly added `PasswordAccountAuthenticator` accepts only pre-provisioned account records with cryptographic password verifiers. Per-user random 16-byte salts, 600,000 PBKDF2-HMAC-SHA256 iterations, and 32-byte derived keys are stored in a versioned `v1` format. Unsupported work factors and malformed records fail closed. The verifier performs fixed-time comparison and dummy PBKDF2 work for unknown accounts. Default JSON serialization and record text formatting redact verifier values.

The credential envelope is an internal `player-id + newline + password` contract, **not** a remote protocol. Production must provide a reviewed TLS-only ingress, credential provisioning/database, throttling and lockout, reset/recovery and secure secret lifetimes. Legacy plaintext password bytes are not accepted or migrated by this code. Only synthetic test identities are provisioned.

## Deliberate deferrals

The system cannot yet serve remote players. Missing: real authenticator and credential lifecycle, TLS, secure ingress/identity binding, token replay prevention across a disconnected/stolen device, durable offline chat, cross-device revocation, password reset, room permissions, DMs/channels, rate limits, moderation, telemetry redaction, storage, and protocol interoperability. Tokens are per-process only and cannot survive restart. A reconnect after token expiry requires fresh authentication and rejoining without historic replay.

No confidential or real credentials, live data, database account migrations, or Internet endpoints are used in these tests.

## Verification

`tests/Arthea.Session.SmokeTests` exercises identity forgery rejection, capacity, duplicate sessions, suspend/resume, token rotation and old-token invalidation, expired token cleanup, replay after disconnect, room movement, fresh-session replay restrictions and a concurrent resume race. Run `bash scripts/verify-modern-core.sh` from a supported .NET 10 environment.

Publication remains blocked by the historical secret/licensing audit in #5. A passing test here does not authorize deployment.

## Loopback TLS transport prototype

A separate `Arthea.Transport` assembly binds **only to IPv4 loopback** on an ephemeral port and requires a server certificate with its private key before startup. It accepts TLS 1.2/1.3 connections through `SslStream`, has an explicit connection cap, handshake/read deadlines, strict UTF-8 and 1024-byte line bounds, and suppresses accidental exception/credential output.

The proof protocol is line-based: `LOGIN name`, `PASS password`, `SESSION token`, `LOOK`, `NORTH`/`SOUTH`, `SAY text`, `POLL cursor`, `RESUME token`, and `QUIT`. **This is not Telnet/GMCP** and cannot yet be used as evidence that MudForge or Mudlet can connect. A future supported client protocol must be selected and reviewed separately (#1/#2).

The only TLS certificate and account credentials in the test are synthetic and generated/initialized in memory. Tests validate successful TLS transport, rejected self-signed server certificates without explicit trust, room isolation, two-player conversation, and reconnect token rotation.

**Do not expose this listener publicly** or use test certificate-pinning behavior as a production trust policy. The library lacks production TLS lifecycle, durable sessions, rate limiting, password provisioning, identity recovery, GMCP, browser/WebSocket transport, auditing, and the operational controls described in #5.

## Bounded outbound writes

In addition to handshake and input deadlines, every application-level TLS write
(including the initial greeting, session response, room metadata, chat replay
and Telnet/GMCP negotiation replies) has an independent 5-second default
deadline. The loopback-only server exposes a validated `writeTimeout` injection
(100 ms–24 hours) for controlled tests. A stalled receiver cannot pin a
connection's write indefinitely; cancellation closes that connection and
suspends its session for bounded reconnect.

Each **individual write** has a deadline; this does not impose one cumulative
deadline across a multiple-message response or add outbound bandwidth quotas.
The socket tests validate configuration bounds and exercise regular text and
GMCP paths, but do not constitute reproducible real-network slow-reader load
qualification. No public listener or release authorization is implied.

## Authenticated idle and partial-command deadlines (#11)

The loopback-only test transport now distinguishes TLS handshake (still 5 seconds), unauthenticated first-byte/read admission (default 15 seconds), authenticated idle time between complete commands (default 15 minutes), and an incomplete-command or unfinished GMCP/Telnet frame (default 15 seconds **from first input byte**, never extended by extra fragments). Completed Telnet negotiation frames return to the idle state, while incomplete control frames stay bound by the partial deadline. All read deadline knobs are constructor-injected for deterministic short socket tests, validated to 100 ms–24 hours; the default still fails closed and no public endpoint is enabled.

An expired session coordinator bearer remains invalid regardless of socket liveness (default 15-minute absolute session TTL): longer idle socket policy does not constitute indefinite authorization. Reconnect still requires the correct suspended token. Production must choose an operational heartbeat/refresh design, explicit rate limits and protected session exchange; this socket test is not a deployable server.
