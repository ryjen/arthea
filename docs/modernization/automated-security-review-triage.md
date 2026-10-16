# Independent automated security review — scoped triage (2026-10-16)

## Scope and evidence

The locally hosted independent **Supervisor Gateway** was contacted over its
Tailscale HTTPS endpoint through Desktop Commander and
given **read-only** modern source at the exact `main` revision
`7c7c3c6c7cc920034b9d4b9aa72efbf554b60d18`:

- `Arthea.Core/Social/PasswordAccountAuthenticator.cs`,
  `RoomSessionCoordinator.cs`, `RoomChatRuntime.cs`
- `Arthea.Transport/LoopbackTlsGameServer.cs`
- `Arthea.Transport/TelnetGmcpCodec.cs`

The initial large review requests failed (one empty response; one timeout).
A minimal `/no_think` calibration succeeded; bounded per-file requests then
returned findings. These are **automated suggestions requiring source-based
adjudication**, not a human/independent security sign-off or a claim of
complete analysis. Only scoped repository source was sent; no credentials or host-execution
instructions were included.

## Adjudication

| Reviewer assertion | Source-backed adjudication |
| --- | --- |
| No auth rate limiting | **Confirmed as a documented limitation**, not a regression: `PasswordAccountAuthenticator.Authenticate` intentionally performs PBKDF2 work, but no ingress admission/rate limiting exists. The prototype listens only on IPv4 loopback. Public ingress remains forbidden until a separate threat model and enforced limits are reviewed |
| `Connect` accepts arbitrary room names | **Not an observed bypass:** `RoomChatRuntime.Enter` verifies `RoomGraph.TryGetRoom` before publishing presence, and throws for unknown IDs |
| Four token-generation attempts permit predictable/colliding bearers | **Unsupported:** `RandomNumberGenerator.GetBytes(32)` generates unpredictable 256-bit bearer material; `NewToken` hashes tokens and rejects an existing digest. Four attempts is a bounded fail-closed allocation policy, not a collision exploit |
| Chat text lacks safety validation | **Contradicted:** `RoomChatRuntime.Say` enforces nonblank, maximum length and `VisibleText.IsSafe`, with negative Unicode/control tests |
| Incomplete Telnet input resets its deadline indefinitely | **Contradicted:** `LoopbackTlsGameServer.Receive` switches to the incomplete-input deadline at the **false → true** partial-state transition, not every byte; existing live socket tests exercise partial-input timeout |
| Plain TLS input allocates an unbounded line | **Contradicted:** `ReceivePlain` imposes a 1,024-byte ceiling and fails closed; existing live oversize-line socket test |
| TLS handshake has no deadline | **Contradicted:** `AuthenticateAsServerAsync` receives a linked cancellation token with a five-second handshake deadline |
| Telnet or GMCP malformed UTF-8 is not rejected | **Contradicted:** both `EmitLine` and `ParseGmcp` use strict UTF-8 decoding and convert `DecoderFallbackException` to `InvalidDataException`; expanded direct codec tests now pin both paths |
| Empty or oversized GMCP commands/module names accepted | **Contradicted:** command names require 1–64 ASCII letters/periods; module declarations validate token count/name length and allowed characters. New targeted tests pin empty, overlong and whitespace-only cases |

**Additional test-design finding (confirmed):** Earlier `Arthea.Gmcp.SmokeTests`
sent multiple malformed subnegotiations through the **same decoder** after
the first error. The decoder is not reusable after a thrown parse error
because the socket is supposed to close. Subsequent expected exceptions
could therefore arise from stale parser state rather than the intended
invalid input. Each negative test now uses a fresh negotiated codec, except
the intentional atomic declaration-state assertion on one instance.

This is a targeted verification improvement, not evidence of a new exploitable
production vulnerability. No production source changes or public deployment
are authorized by this review.

## Residual gates

Issue [#12](https://github.com/ryjen/arthea/issues/12) stays open.
Before a public-facing service is even considered, obtain separately
qualified independent source/security review with reproducible evidence of
authentication rate limits, transport admission/abuse protections,
certificate policy, session ownership, token display/recovery,
GMCP interoperability and failure containment. Historical publication
provenance remains tracked separately in
[#5](https://github.com/ryjen/arthea/issues/5). The original legacy server
remains offline and unsuitable for Internet deployment.
