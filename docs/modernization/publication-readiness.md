# Arthea publication readiness

**Current status: BLOCKED. Keep the source private and do not expose the legacy game or web app publicly.**

This is a scoped risk inventory, not a security certification. Modernization stays in the existing repository, preserving Git history where safe.

## Evidence confirmed from the original code

- Server, editor and test configs contained embedded database credentials and obsolete hosted MongoDB settings.
- Telnet authentication stores and compares raw password bytes without TLS.
- The old ASP.NET web configuration enables debugging and detailed error output.
- Vendored ThirdParty binaries, source, samples and licenses have not been fully audited.
- No supported modern .NET build/test/release pipeline is verified.

## Initial configuration hygiene slice

- Remove connection strings from the three currently tracked application configs.
- Require ARTHEA_MONGODB_URI from the environment and fail closed if empty or lacking a database name.
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

## Validation limits

Current-file configuration checks do not prove clean history, licenses, safe authentication, or functional compatibility. See [issue #5](https://github.com/ryjen/arthea/issues/5).
