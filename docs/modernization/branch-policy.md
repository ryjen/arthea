# Arthea branch contract

**Effective October 12, 2026 (Pacific).**

| Branch | Purpose | Updates |
| --- | --- | --- |
| `master` | Original .NET Framework/MongoDB/Telnet project, at original October 2015 commit `62bf4162b686fd1db64a43cc495e7e81868d1679` | **Preserve unchanged**. No modernization merges or new work. |
| `main` | Legacy project with optional .NET 10 modernization experiments, retaining prior upgrade history | **All optional maintenance/modernization experiments**, fixes, reviews, PRs and local qualification target here. |

The modernization branch started as an **exact copy** of the prior `master` head `fa14d3b5a8942dbb3d59f8a7c1e5237994d864b1`. The `master` ref was subsequently returned to the original legacy commit using an exact expected-head lease; no modernization commits were deleted because `main` retains that full ancestry.

**Scope:** This remains a historical hobby project, not an actively maintained game or product roadmap. Modernization is optional engineering practice, not a promise of release, support, or deployment.

**Contributor checklist:** base feature PRs on `main`, target `main` explicitly when creating a PR, verify exact candidate SHA and scope, then merge only into `main`. Never merge modernization back into `master`. The modern project's GitHub CI trigger is also scoped to PRs targeting `main`, though actual GitHub-hosted runner admission is not yet qualified (#12). Changing the GitHub *repository default branch* from `master` to `main` is an administrative setting, separate from simply creating the branch; verify it in GitHub Settings.

**Safety:** The repository stays **private**. Historical MongoDB credentials, raw legacy password/Telnet flow, deleted SQL dump and unknown licensing remain blockers (#5); resetting the `master` tip does not purge historic Git data. Do not host the old server publicly, make either branch public, claim production approval, or assume authentication/restart recovery is complete. Existing full-source publication and independent security review (#12) gates still apply.

Before any automated branch reconciliation, compare the live branch heads: `master` must stay at the original legacy commit and `main` must be a descendant of the old upgraded baseline. Treat any ref drift as requiring fresh review, never as authorization to force overwrite.
