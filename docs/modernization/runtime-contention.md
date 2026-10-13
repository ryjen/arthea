# Room runtime synchronization — exploratory measurement

The modern `RoomChatRuntime` currently protects presence and journal calls using a single
process-wide lock. This makes membership transitions and `Say`/`Replay` authorization
linearizable within one runtime instance, but an injected SQLite adapter performs I/O while
that lock is held. This can serialize independent rooms and make latency tail-sensitive.

## Reproducible manual probe

With .NET 10 installed:

```sh
dotnet run --configuration Release --project samples/Arthea.ContentionProbe
```

The probe uses a deterministic number of in-memory chat writes and reports wall-clock
throughput for 1, 2, 4 and 8 workers distributed over eight rooms. It is **not**
a load test or latency benchmark: JIT warmup, CPU scheduling, memory allocation,
host load, iteration scale and GC can distort comparisons. No numeric thresholds
are CI admission criteria. The probe does not simulate persistent SQLite I/O.

## Decision gate

Do not replace the global lock merely because throughput plateaus. First obtain repeated
host-specific measurements (including p95/p99 response times with the SQLite adapter),
a concrete performance requirement and a contention profile. A split-lock design must
preserve membership-epoch authorization across concurrent move, leave, say and replay.
Naively moving the journal call outside the lock can authorize reads after leave or
attribute a write to the wrong room. The existing tests (#19, #23–#25) are necessary
but not sufficient for a distributed/multi-process design.

This remains a legacy project exercise, not a production server qualification.
