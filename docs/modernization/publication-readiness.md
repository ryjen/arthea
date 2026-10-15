# Arthea publication readiness

**Publication/deployment qualification: BLOCKED.** As observed October 14, 2026, the repository is already **public**. Public visibility is not evidence of permission, historic secret revocation or safe deployment. Do not deploy the legacy game or web app; any visibility change requires separate owner authorization.

This is a scoped risk inventory, not a security certification. Modernization stays in the existing repository, preserving Git history where safe.

## Evidence confirmed from the original code

- Server, editor and test configs contained embedded database credentials and obsolete hosted MongoDB settings.
- Telnet authentication stores and compares raw password bytes without TLS.
- The old ASP.NET web configuration enables debugging and detailed error output.
- Vendored ThirdParty binaries, source, samples and licenses have not been fully audited.
- Modern .NET 10 experimental CI has passed exact-head pull-request checks (e.g. [PR #37](https://github.com/ryjen/arthea/actions/runs/38370538842)); the original .NET Framework server and any public release/deployment pipeline remain unqualified.

## Initial configuration hygiene slice

- The three tracked legacy application configs were sanitized at current tree (verified by the bounded config hygiene test); this does not resolve earlier Git exposure.
- Modernized configuration requires ARTHEA_MONGODB_URI from the environment rather than source-controlled credentials.
- Ignore local environment/override files and document isolation requirements.

**This does not remove historic credentials, revoke them, fix the login protocol, or clear the repository for public visibility.**

## Hard public source release gates

1. Scan all reachable history, tags, branches, assets, workflows, and current files for secrets, PII, internal infrastructure details and redistribution limitations. Never copy secret values into PRs or issues.
2. Confirm historical credentials revoked/expired; rotate if active. Preserve private evidence.
3. Review minimally disruptive historical preservation/remediation options, including clone/fork impacts, if secrets or unlicensed third-party material cannot be made public in prior commits.
4. Inventory licenses, origin, versions and permissions for all vendored DLLs/source/sample content. Establish a license for Arthea-owned code only after ownership review.
5. Independently review the **exact** publishable state, documentation, test evidence and Git ancestry; require explicit approval before changing visibility.

## Additional gates for an Internet multiplayer deployment

- Hash passwords securely and design credential reset/migration for legacy accounts.
- Enforce client-verifiable TLS on all authenticated connections; test invalid certificates and downgrade paths.
- Constrain connection count, input size, timeouts, command handling and scripting; prevent private chat and account data leakage.
- Establish reproducible builds, layered tests, secure deployment defaults and rollback procedures.

## Scoped local checks

    python3 tools/check_config_hygiene.py
    python3 -m unittest discover -s tools/tests -v

These verify selected current application XML configuration files only; they do not scan history or verify the C# runtime.

## Bounded historical secret-pattern inventory

    python3 tools/audit_git_history.py

This read-only command walks unique reachable Git blobs and prints **category counts only**, never matched secret values. It deliberately exits nonzero for suspected credentials, private-key markers, or uninspected oversized/binary objects. Its results require independent triage: code examples can create false positives; data can be sensitive without matching patterns; deleting a file from HEAD cannot erase older commits; scan results do not prove credential revocation, license rights or publication safety.

## Validation limits

Current-file configuration checks do not prove clean history, licenses, safe authentication, or functional compatibility. See [issue #5](https://github.com/ryjen/arthea/issues/5).
