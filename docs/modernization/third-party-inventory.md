# Arthea — preliminary third-party redistribution inventory

**Status: INCOMPLETE. No redistribution clearance or license grant is claimed.**

Historical baseline from the tracked file tree of draft PR #6 and a bounded review of license-named files; counts are not a refreshed October 2026 inventory. The repository is already public as of the October 14 review, but independent artifact-level redistribution permission is **not** established. Do not treat visibility as clearance or silently remove/rewrite historical assets.

| Location | Tracked files | Evidence and question | Next action |
| --- | ---: | --- | --- |
| `ThirdParty/Thought-Terminals/` | 123 | Source README and assembly metadata identify **version 0.1.1**, David Pinch (2007–2009). `FILE_ID.DIZ` describes open source but cautions against general distribution; no license-named file identified. **No precise redistribution grant established.** | Locate an authoritative license/grant for version 0.1.1; verify separately vendored DLL and sample art/content. |
| `ThirdParty/Thought-Net-Telnet/` | 65 | Included source explicitly identifies version **0.2**, David Pinch (2007), and **LGPL-2.1-or-later**; the matching LGPL 2.1 license text is committed under the source project. Binary-to-source equivalence and notice obligations are not yet verified. | Verify the separately vendored DLL against this source/version; confirm modifications, copyright notices, distribution conditions and source obligations. |
| `ThirdParty/Jint/` | 5 | Historic Jint and Antlr binaries/debug symbols; no local license found. | Identify provenance, versions and exact license obligations; replace or remove obsolete binaries where possible. |
| `ThirdParty/MongoDB/` | 2 | Historic MongoDB .NET driver binaries without adjacent license. | Confirm binary versions and redistributable rights; prefer supported package reference in new adapter. |
| `ThirdParty/NLog.dll` | 1 | Historic logger binary without adjacent license. | Confirm upstream provenance/version/license; replace with supported package or platform logging. |
| `ArtheaEngine.Tests/ThirdParty/NUnit/` | 2 | NUnit test assemblies without adjacent license. | Replace vendored binary test harness; separately assess source/history redistribution. |
| `ArtheaServer/Libraries/` | 1 | Microsoft task scheduler binary. | Determine original distribution terms; isolate or remove obsolete scheduling integration. |
| `ArtheaServer/ThirdParty/` | 4 | Additional vendored Thought.Net/Thought.Terminals binaries and notes. | Verify duplicates, source/binary consistency, redistribution requirements. |

## Source-level license evidence — October 15, 2026

The tracked source is stronger evidence than an assumed license based on the
library's name, but it does **not** establish the provenance of separately
bundled DLLs:

- **Thought.Net.Telnet**: `ThirdParty/Thought-Net-Telnet/Solution/Thought.Net.Telnet/ReadMe.txt`
  names David Pinch and version 0.2 and explicitly grants GNU Lesser General
  Public License **2.1 or later**. The matching
  `Solution/Thought.Net.Telnet/License.txt` contains LGPL version 2.1;
  `Properties/AssemblyInfo.cs` reports assembly version `0.2.0.0`.
  `ArtheaServer/ThirdParty/Thought.Net.Telnet/Release/Thought.Net.Telnet.dll`
  is a **separate binary** requiring independent hash/version/build matching.
  Source-level identification is **not completed binary provenance**.
- **Thought.Terminals**: `ThirdParty/Thought-Terminals/ReadMe.txt` names David
  Pinch, and `Solution/Thought.Terminals/AssemblyInfo.cs` declares version
  `0.1.1.0`. `FILE_ID.DIZ` claims "free and open source" while also stating
  "not intended for general distribution." These descriptive statements are
  **not a specific verifiable redistribution license**. Until a grant is
  located, keep source, samples, and the separately bundled
  `ArtheaServer/ThirdParty/Thought.Terminals/Release/Thought.Terminals.dll`
  in the *unresolved* category.

Use the optional committed `--sha256-jsonl` inventory to verify artifacts
against authoritative releases if available. Do not substitute a reconstructed
version number, source tree, or unrelated fork's license for that verification.
No library was relicensed, removed, or declared cleared in this checkpoint.

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

## Exact artifact fingerprinting (optional)

`python3 tools/inventory_third_party.py --sha256-jsonl` emits one JSON object per
regular tracked vendor blob in **HEAD**, with its vendor group, repository path,
size and SHA-256 digest. It uses Git's committed blob rather than potentially
modified working-tree bytes, excludes symlinks and submodules, and errors rather
than silently skipping an oversized regular blob (16 MB ceiling per artifact).
The ordinary, inexpensive CI inventory still prints only category counts.

Use a locally retained fingerprint report to compare a *specific* vendored
binary/source snapshot with an independently retrieved upstream release, rather
than assigning licenses from filenames or product names. Hashes identify bytes;
they do not prove who authored them or that public redistribution is permitted.
No artifact uploads or third-party network queries occur in this command.

An [older public Jint source fork](https://github.com/ImagineLearning/Jint)
contains a BSD-2-Clause license, but it is only an **unmatched candidate** for
provenance research. Without a corresponding version and verified artifact
fingerprint, **do not attribute that license** to `ThirdParty/Jint/` binaries
or their bundled Antlr components.

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
