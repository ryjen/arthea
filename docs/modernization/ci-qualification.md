# Arthea CI qualification

This document covers **private repository development validation only**, not release, Internet hosting, licensing clearance, or approval of historical secrets (#5).

## Entrypoint and reproducibility

The project-owned `flake.nix` declares the Linux x86-64 .NET 10 SDK and Python 3 dependencies. The committed `flake.lock` pins NixOS/nixpkgs to a specific reviewed nixpkgs commit that includes **.NET SDK 10.0.401** (GitHub revision `8edc0c72e3a38faf5434e40d1f19431125fe4b30`; lock's NAR hash cross-checked against an existing published `nix-community/nixvim` flake.lock). The root `global.json` pins SDK 10.0.401, and the synthetic BSON project retains its locked NuGet restore. A mismatch between the pinned SDK and the declared Nix package is a **failure** to resolve by reviewing/updating the lock, not by weakening the SDK requirement.

Local or CI invocation (requires Nix with flakes enabled):

```bash
nix develop .#ci --command bash scripts/verify-modern-core.sh
```

CI uses the same command, checks the exact GitHub SHA before validation, executes all seven independent executable suites (including real loopback/TLS/GMCP socket tests), the local demo self-check, all nine Python regression tests, and current tracked-config hygiene. Historical Git secret/PII clearance and old .NET Framework application builds are **not** asserted.

## Trigger, cost and trust

- One stable required-check candidate: **`arthea / verify (linux)`**.
- The workflow runs on pull requests targeting `master` and by explicit manual dispatch. It does **not** duplicate every successful PR run with automatic post-merge `push` validation. For candidate composition, GitHub PR's synthetic merge SHA is checked exactly; any required future merge-queue integration needs a separately reviewed `merge_group` event.
- Uses an isolated GitHub-hosted Ubuntu 24.04 runner, not privileged/persistent Dubnium infrastructure. The fork-source guard runs **before checkout** and fails the job rather than skipping a check. No `pull_request_target`.
- No package/publish/deploy steps, artifacts, cache uploads, privileged environment, or writes to repository content. Token scope is `contents: read`; checkout does not persist authentication. Third-party actions are pinned to immutable commits. Nixpkgs is pinned.
- Concurrency cancels superseded PR jobs; manual dispatch jobs are independent. The single job has a 35-minute bound. This deliberately uses GitHub-hosted compute for trust isolation; validate the actual billing and cold Nix closure cost before adding more matrices or triggers.

## Admission and evidence

A new workflow file is **not** a passing CI run. First review its diff and toolchain lock, check the actual workflow run on the exact PR synthetic-merge revision, diagnose package/version or runner/bootstrap failures without bypass, then apply the explicit `arthea / verify (linux)` required-check name in branch rules **after** evidence and permission review. Changing repository protections is a separate consequential action.

CI does not provide an independent source/security review. Issue #12 remains open until a qualified second reviewer evaluates session token exposure, TLS, GMCP bounds, credential handling and production gating, and the appropriate branch restrictions/checks are configured. Manual Mudlet/MudForge acceptance, production identity/recovery, third-party license provenance and historical secret remediation are separate release gates.

## Initial GitHub runner admission failure

Initial PR #14 run [38321982689](https://github.com/ryjen/arthea/actions/runs/38321982689) failed in approximately 3 seconds with **zero job steps, no assigned runner (runner_id=0) and no obtainable job log**. This occurred before the Nix toolchain or tests ran. GitHub billing/spending limits, runner availability or account configuration should be reviewed in the Actions UI; **the cause is unverified**. Repeating the same run or changing security boundaries is not a diagnosis. The first pinned Nix lock also contained SDK **10.0.400**, which was a separate latent correctness defect and has now been changed to a hash-verified pinned revision shipping **10.0.401**. GitHub CI remains unqualified until a real job reaches the test step and passes.
