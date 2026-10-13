# Modern test harness — incremental migration boundary

Arthea is a historical archive. The executable .NET 10 smoke suites under
`tests/` remain a reproducible regression baseline via
`bash scripts/verify-modern-core.sh`. Do not port old Framework-era applications
just to adopt a test framework.

## Current gap

Many independent assertions live in each executable `Program.cs`, making
failures less granular and setup/teardown harder to reuse. The existing CI
builds and runs these suites; they currently provide meaningful regression
protection without depending on a test runner.

## Smallest safe migration

1. Migrate one bounded suite (prefer pure `Arthea.Core` invariants) to a
   conventional per-case test runner while preserving existing smoke coverage.
2. Pin framework/test SDK dependencies in the repo's Nix/lockfile environment.
   No unbounded package updates. Prefer test fixtures with deterministic seeds,
   no network access or secrets and independently named assertion cases.
3. Run the new suite in `scripts/verify-modern-core.sh` and CI. Preserve the old
   executable until the new suite proves equivalent coverage on exact-head runs.
4. Require explicit fixture ownership for temp files, SQLite journals, sockets,
   certificates and cancellation tokens. Failed tests should isolate resources.
5. Keep concurrency behavioral checks separate from latency benchmarks. Do not
   introduce timing-based CI pass/fail thresholds or change runtime locking.

## Acceptance for a future implementation PR

Per-test identity in runner output; deterministic retry; equivalent negative and
positive cases; locked restore; no deployment dependencies; no changes to legacy
`master`. This document alone does not imply those changes were implemented.
