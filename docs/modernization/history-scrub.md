# History sanitization — October 2026

This legacy hobby repository uses two branch heads: a sanitized archival `master` and a modernization-experiment `main`.

The Git histories for both branches were rewritten to replace the authenticated historical MongoHQ database connection URI and remove all historical `arthea.sql` and `sqldump.bat` blobs. Their old Git commit IDs are invalidated. The legacy branch retains its 2015 source layout but is not byte-for-byte the original version. Modernized application source and tests on `main` are unchanged except for documentation reflecting the new branch IDs.

The rewrite was performed in an isolated mirror with an owner-only local rollback bundle retained for recovery. **Do not reintroduce history from that bundle or old clones** by merging or pushing; use a fresh clone after a scrub. The rollback bundle contains sensitive pre-rewrite history and must remain access-restricted until it is intentionally destroyed.

GitHub pull-request refs, cached commit views, forks, external clones and third-party indexes cannot be erased by pushing rewritten branch refs. GitHub Support may need to remove server-side cached references. Rewriting Git does not revoke leaked MongoHQ or old SQL-backup credentials; verify provider shutdown or rotation separately (issue #5). This document does not grant permission to expose the legacy server or claim that historical vendor assets are fully redistributable.
