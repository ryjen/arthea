# Arthea branch contract

**Effective October 12, 2026 (Pacific).**

| Branch | Purpose | Updates |
| --- | --- | --- |
| `master` | Sanitized preservation snapshot of the original .NET Framework/MongoDB/Telnet project, at rewritten commit `3a822e93b5811d291ffd7b2f70122d0e171c5817` | **Preserve the sanitized legacy snapshot**. No modernization merges or new work. |
| `main` | Legacy project with optional .NET 10 modernization experiments, retaining prior upgrade history | **All optional maintenance/modernization experiments**, fixes, reviews, PRs and local qualification target here. |

Both branches were rewritten in October 2026 to remove an embedded historical MongoDB credential and old SQL-dump/backup-script artifacts. As a result, the legacy `master` is a **sanitized preservation copy**, not byte-for-byte identical to the 2015 Git commit. The upgraded history remains on `main`.

**Scope:** This remains a historical hobby project, not an actively maintained game or product roadmap. Modernization is optional engineering practice, not a promise of release, support, or deployment.

**Contributor checklist:** base feature PRs on `main`, target `main` explicitly when creating a PR, verify exact candidate SHA and scope, then merge only into `main`. Never merge modernization back into `master`. The modern project's GitHub CI trigger is also scoped to PRs targeting `main`, and recent exact-head GitHub-hosted runs succeeded (e.g. [PR #37](https://github.com/ryjen/arthea/actions/runs/38370538842)); this does not qualify release security (#12). Changing the GitHub *repository default branch* from `master` to `main` is an administrative setting, separate from simply creating the branch; verify it in GitHub Settings.

**Safety (October 14, 2026):** The repository was observed **public**, and no visibility change is authorized by this document. Historical MongoDB credentials, raw legacy password/Telnet flow, deleted SQL dump and unknown licensing remain blockers (#5); resetting the `master` tip does not purge historic Git data. Do not host the old server publicly, interpret public visibility as a clearance decision, claim production approval, or assume authentication/restart recovery is complete. Existing full-source publication and independent security review (#12) gates still apply.

Before any automated branch reconciliation, compare the live branch heads: `master` must stay at the sanitized legacy commit and `main` must preserve the scrubbed modernization history. Treat any ref drift as requiring fresh review, never as authorization to force overwrite.
