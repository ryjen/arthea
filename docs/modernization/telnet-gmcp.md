# Telnet/GMCP framing contract (prototype)

The `TelnetGmcpCodec` in `Arthea.Transport` is a per-connection, bounded parser and serializer for RFC-854 Telnet control bytes and GMCP option 201. It **does not run a listener** and is not wired into the existing loopback-only TLS text game server yet.

- Server offers `IAC WILL 201`; a client must opt in with `IAC DO 201`.
- `Core.Supports.Set`, `Add` and `Remove` govern optional `Room.Info`; text-only clients remain supported by the independent text path.
- `Room.Info` emits stable opaque room IDs, name, directional exits, JSON-escaped values, no coordinates fabricated, and no account/chat/private data.
- Input parsing spans fragmented chunks and rejects malformed or oversized input. The codec never interprets GMCP content as gameplay commands.
- Session state and negotiation must be restarted after reconnect; disabled GMCP yields no metadata.
- Unknown Telnet commands/options are either rejected or ignored within bounded behavior, not treated as credentials.

**Not delivered:** true Telnet client/server socket integration, GMCP synchronization with authenticated room movement, secure login from Mudlet/MudForge, proper terminal negotiation (ECHO, SGA, NAWS), client scripts or mapper adaptation, GMCP fuzz/incremental resource tests, and Android/iPhone acceptance. A real client must connect through an authenticated encrypted transport and have the same authoritative room state as text output before claiming interoperability.

The original insecure Telnet server remains unsuitable for public deployment. This contract is not deployment or publication approval.
