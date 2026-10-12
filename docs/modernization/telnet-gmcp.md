# Telnet/GMCP framing contract (prototype)

The `TelnetGmcpCodec` in `Arthea.Transport` is a per-connection, bounded parser and serializer for RFC-854 Telnet control bytes and GMCP option 201. It is now used by the existing loopback-only TLS text game server when explicitly enabled with `enableTelnetGmcp: true`. The default remains the original line-based test mode. This is still an experimental local adapter, not an exposed Telnet listener.

- Server offers `IAC WILL 201`; a client must opt in with `IAC DO 201`.
- `Core.Supports.Set`, `Add` and `Remove` govern optional `Room.Info`; text-only clients remain supported by the independent text path.
- `Room.Info` emits `num` (the default MudForge mapper field) and `id` as the same stable **string** room ID, plus `name` and six canonical directional `exits` with target IDs. Room IDs are not assigned synthetic numeric vnums. Optional `zone`, `terrain`, and `coord` are absent until the world actually models them; some Mudlet mapper scripts demand numeric `num` and will need a separately tested adapter. No account/chat/private data is included.
- Input parsing spans fragmented chunks and rejects malformed or oversized input. The codec never interprets GMCP content as gameplay commands.
- Session state and negotiation must be restarted after reconnect; disabled GMCP yields no metadata.
- Unknown Telnet commands/options are either rejected or ignored within bounded behavior, not treated as credentials.

**Not delivered:** true Telnet client/server socket integration, GMCP synchronization with authenticated room movement, secure login from Mudlet/MudForge, proper terminal negotiation (ECHO, SGA, NAWS), client scripts or mapper adaptation, GMCP fuzz/incremental resource tests, and Android/iPhone acceptance. A real client must connect through an authenticated encrypted transport and have the same authoritative room state as text output before claiming interoperability.

The original insecure Telnet server remains unsuitable for public deployment. This contract is not deployment or publication approval.

## Live socket proof

The opt-in loopback TLS server sends `IAC WILL 201`, processes fragmented Telnet negotiation and `Core.Supports`, and emits opt-in `Room.Info` on login, `LOOK`, and room movement. The same test proves a client that never sends `DO 201` receives only ordinary room text. The GMCP room ID is obtained from the authenticated session's authoritative world state; it is not accepted from the client.

This is **not yet verified against real Mudlet/MudForge clients**, which may require Telnet terminal options, more careful prompt/echo handling, or different authentication UX. A displayed bearer token is inappropriate for production terminal scrollback; production must use a separately reviewed account/session protocol. Nothing here authorizes deployment or changes the existing legacy server.

## Mapping profile decision

MudForge's documented default room identity is `Room.Info.num`, while `id` is an optional alternative mapping. This prototype emits both using the same opaque ID. A generalized Mudlet GMCP consumer can read the string; numeric-only third-party Mudlet mappers are **not** automatically supported. Avoid inventing numeric identifiers from hashes or indices, which could collide or change after world edits. Define persistent numeric VNUMs in the data model only if numeric-only mapper support becomes a required milestone. Official client testing has not occurred.

## Review hardening — ignored Telnet frames and malformed packets

All parsed and ignored Telnet subnegotiation frames now have a **cumulative 4096-byte ceiling per frame**, even when split across multiple network reads. Unknown options are still ignored when bounded, but oversized ignored content fails the connection closed. Malformed GMCP and other `InvalidDataException` decode failures are contained to the affected TLS handler without propagating to the listener. Contract and socket-negative tests verify both properties; this is not yet a full protocol fuzzer or an Internet deployment authorization.
