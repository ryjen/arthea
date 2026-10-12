#!/usr/bin/env python3
"""Conservative historical credential-pattern inventory.

Runs against reachable Git objects, reports only categories and counts.
Not a complete security/PII audit and never evidence of credential revocation.
"""
from collections import Counter
from pathlib import Path
import re
import subprocess
import sys

REPO = Path(__file__).resolve().parents[1]
MAX_BYTES = 2_500_000
PATTERNS = {
    "authenticated_url": re.compile(rb"(?i)\b(?:mongodb(?:\+srv)?|https?|postgres(?:ql)?|mysql)://[^/\s:@]+:[^/\s@]+@"),
    "password_assignment": re.compile(rb"(?i)\b(?:password|pwd)\s*=\s*[^\s;\"'<>]+"),
    "private_key_marker": re.compile(rb"-----BEGIN (?:[A-Z ]*PRIVATE KEY)-----"),
}

def git(repo, *args):
    return subprocess.check_output(
        ["git", "-C", str(repo), *args], stderr=subprocess.DEVNULL
    )

def audit(repo=REPO):
    # Each reachable blob is inspected once. The same content may exist at several
    # paths; rev-list --objects displays only one path for a shared object.
    lines = git(repo, "rev-list", "--objects", "--all").decode("utf-8", "replace").splitlines()
    seen = set()
    counts = Counter()
    for line in lines:
        oid = line.split(" ", 1)[0]
        if oid in seen:
            continue
        seen.add(oid)
        if git(repo, "cat-file", "-t", oid).strip() != b"blob":
            continue
        if int(git(repo, "cat-file", "-s", oid)) > MAX_BYTES:
            counts["skipped_oversized"] += 1
            continue
        contents = git(repo, "cat-file", "blob", oid)
        if b"\x00" in contents[:1024]:
            counts["skipped_binary"] += 1
            continue
        counts["checked_text_blobs"] += 1
        for category, pattern in PATTERNS.items():
            if pattern.search(contents):
                counts[category] += 1
    return counts

def main():
    try:
        counts = audit()
    except (subprocess.CalledProcessError, OSError, ValueError):
        print("Git history audit could not inspect all requested objects.", file=sys.stderr)
        return 2
    for category in ("checked_text_blobs", "authenticated_url", "password_assignment",
                     "private_key_marker", "skipped_binary", "skipped_oversized"):
        print(f"{category}: {counts[category]}")
    print("Scope: reachable Git blobs only; values suppressed; manual verification required.")
    # Deliberately fail on candidates or incomplete binary/large-blob coverage;
    # callers must not mistake a weak pattern scan for publication clearance.
    findings = any(counts[x] for x in (
        "authenticated_url", "password_assignment", "private_key_marker",
        "skipped_binary", "skipped_oversized",
    ))
    return 1 if findings else 0

if __name__ == "__main__":
    raise SystemExit(main())
