# Modern room metadata — local validation exercise

Arthea remains a legacy hobby project. This is a bounded .NET 10 clean-core and
security exercise, not a production server or new game roadmap.

RoomGraph construction is the single ingestion boundary for fresh-world
rooms and LegacyRoomProjection snapshots. Room IDs appear directly in
line-oriented protocol replies, so allowing newlines, terminal control bytes,
or invisible bidirectional overrides permits fabricated client output and
misleading displays. Metadata is now validated *before* the graph is indexed
or any movement/chat command is executed.

Rules: room IDs, names and exit destinations must be nonblank and at most
256 UTF-16 code units, with no control characters or Unicode Format,
LineSeparator or ParagraphSeparator code points. Identifiers remain opaque
and ordinary non-ASCII labels remain supported. Exit destinations must also
exist in the graph. The constructor still defensively copies the caller's
room and exit dictionaries.

Negative tests cover CR, LF, ANSI escapes, bidirectional overrides,
Unicode line separators, oversized identifiers and bad exit targets.
The original legacy master code is not modified.

This is not a replacement for authenticated sessions, certificate
verification, output encoding, private-chat security, or real-client testing.
See #1, #5 and #7 for other boundaries.
