# Arthea — preliminary third-party redistribution inventory

**Status: INCOMPLETE. No redistribution clearance or license grant is claimed.**

Historical baseline from the tracked file tree of draft PR #6 and a bounded review of license-named files; counts are not a refreshed October 2026 inventory. The repository is already public as of the October 14 review, but independent artifact-level redistribution permission is **not** established. Do not treat visibility as clearance or silently remove/rewrite historical assets.

| Location | Tracked files | Evidence and question | Next action |
| --- | ---: | --- | --- |
| `ThirdParty/Thought-Terminals/` | 123 | Historical source, tests, examples, documentation; copyright attribution in ReadMe. No local license-named file found. | Identify version and authoritative redistribution terms; review bundled sample art/content. |
| `ThirdParty/Thought-Net-Telnet/` | 65 | Source, binaries, samples; contains an LGPL license text. That alone does **not** prove every file is covered or obligations are met. | Confirm upstream license association, copyright notices, modifications and LGPL notices/requirements. |
| `ThirdParty/Jint/` | 5 | Historic Jint and Antlr binaries/debug symbols; no local license found. | Identify provenance, versions and exact license obligations; replace or remove obsolete binaries where possible. |
| `ThirdParty/MongoDB/` | 2 | Historic MongoDB .NET driver binaries without adjacent license. | Confirm binary versions and redistributable rights; prefer supported package reference in new adapter. |
| `ThirdParty/NLog.dll` | 1 | Historic logger binary without adjacent license. | Confirm upstream provenance/version/license; replace with supported package or platform logging. |
| `ArtheaEngine.Tests/ThirdParty/NUnit/` | 2 | NUnit test assemblies without adjacent license. | Replace vendored binary test harness; separately assess source/history redistribution. |
| `ArtheaServer/Libraries/` | 1 | Microsoft task scheduler binary. | Determine original distribution terms; isolate or remove obsolete scheduling integration. |
| `ArtheaServer/ThirdParty/` | 4 | Additional vendored Thought.Net/Thought.Terminals binaries and notes. | Verify duplicates, source/binary consistency, redistribution requirements. |

## Automated current-tree checkpoint — October 14, 2026 (Pacific)

The read-only `tools/inventory_third_party.py` runs as part of
`scripts/verify-modern-core.sh`, and its unit tests passed in
[exact-head CI for PR #39](https://github.com/ryjen/arthea/actions/runs/38695986010).
Its output covered **203 currently tracked files across the eight vendor groups**
and only **one license-named file** (under `ThirdParty/Thought-Net-Telnet/`).
These counts agree with the preliminary group sizes above, but the current-tree
snapshot is **not** an origin, license-content, copyright or historic-ref audit.

```sh
python3 tools/inventory_third_party.py
```

The command prints group-level counts only. A license-named file need not cover
every artifact in its group, and absence of one does not prove that an upstream
license is missing. The public repository still requires independent provenance
and redistribution review; no automatic publishing or deletion is authorized.

## Source/history implications

- Public repository visibility may expose **reachable historic commits and cached pull-request/clone objects**, not only files remaining at HEAD. The scrubbed branch history alone cannot retract previously cached objects.
- Removing DLLs/source from a current branch will not remove them from Git history. If a file cannot be redistributed, preserving original history in the *public* Git graph may be impossible. Preserve a private archival backup before any scoped rewrite decision.
- Review author ownership of Arthea-specific code and assets before choosing a top-level license.
- Do not conflate a vendored dependency's license with an automatic license for the whole repository.

## Remaining evidence to collect

1. Capture authoritative upstream version/license URLs and exact artifact hashes without uploading private binaries or credentials to third parties.
2. Record license texts, attribution, notice/source-offer obligations, and any copyleft/static-linking implications.
3. Audit sample game files, images and documentation separately from libraries.
4. Record disposition per artifact group: keep with attribution, replace, exclude from publishable history, or block publication.
5. Independent legal/provenance and secret-history review of the exact public candidate; publication approval under issue #5.

## Scope limits

Counts cover the current tracked tree, not deleted historic files. Only one license-named file was detected by a filename-based scan; licenses may be described under other names or in upstream material.
