#!/usr/bin/env python3
"""Prevent reintroducing rights-restricted Aurora sample screens in current HEAD.

This is a path-specific, tracked-current-tree check; it does not inspect older
commits, other branches, GitHub pull-request refs, caches, or licenses.
"""
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
RESTRICTED_SCREEN_DIRECTORY = (
    "ThirdParty/Thought-Terminals/Samples/Aurora City/Game Files/Text Files"
)
RESTRICTED_SCREEN_EXTENSION = ".ans"


def restricted_screen(path):
    """Only match actual screen files beneath the specific sample directory."""
    return (
        path.startswith(RESTRICTED_SCREEN_DIRECTORY + "/")
        and Path(path).suffix.casefold() == RESTRICTED_SCREEN_EXTENSION
    )


def check_paths(paths):
    return sorted(path for path in paths if restricted_screen(path))


def committed_paths(repo=ROOT):
    raw = subprocess.check_output(
        ["git", "-C", str(repo), "ls-tree", "-r", "-z", "--name-only", "HEAD"],
        stderr=subprocess.DEVNULL,
    )
    return [os.fsdecode(entry) for entry in raw.split(b"\0") if entry]


def check_current_tree(repo=ROOT):
    return check_paths(committed_paths(repo))


if __name__ == "__main__":
    try:
        findings = check_current_tree()
    except (OSError, subprocess.CalledProcessError) as exc:
        print(f"Unable to inspect committed current tree: {exc}", file=sys.stderr)
        sys.exit(1)
    if findings:
        for path in findings:
            print(f"Restricted Aurora sample screen at HEAD: {path}", file=sys.stderr)
        sys.exit(1)
    print("No rights-restricted Aurora sample screens in current HEAD; history NOT checked.")
