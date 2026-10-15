#!/usr/bin/env python3
"""Inventory tracked vendored file paths without reading their contents.

Filename evidence is not a license grant, vulnerability assessment, or review
of historic commits, external clones and GitHub-managed references.
"""
from collections import Counter
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
SCOPED_ROOTS = (
    "ThirdParty",
    "ArtheaEngine.Tests/ThirdParty",
    "ArtheaServer/Libraries",
    "ArtheaServer/ThirdParty",
)
GROUPS = (
    "ThirdParty/Thought-Terminals",
    "ThirdParty/Thought-Net-Telnet",
    "ThirdParty/Jint",
    "ThirdParty/MongoDB",
    "ThirdParty/NLog.dll",
    "ArtheaEngine.Tests/ThirdParty/NUnit",
    "ArtheaServer/Libraries",
    "ArtheaServer/ThirdParty",
)

def group_for(path):
    for group in GROUPS:
        if path == group or path.startswith(group + "/"):
            return group
    if path.startswith("ThirdParty/"):
        return "ThirdParty/other"
    if any(path.startswith(root + "/") for root in SCOPED_ROOTS):
        return "Other scoped vendored paths"
    return None

def license_named(path):
    name = path.rsplit("/", 1)[-1].casefold()
    return name.startswith(("license", "licence", "copying", "notice", "copyright", "lgpl"))

def summarize(paths):
    counts = Counter()
    notices = Counter()
    for path in paths:
        group = group_for(path)
        if group is None:
            continue
        counts[group] += 1
        if license_named(path):
            notices[group] += 1
    return counts, notices

def tracked_paths(repo=ROOT):
    raw = subprocess.check_output(
        ["git", "-C", str(repo), "ls-files", "-z", "--", *SCOPED_ROOTS],
        stderr=subprocess.DEVNULL,
    )
    return [os.fsdecode(entry) for entry in raw.split(b"\0") if entry]

def main():
    try:
        counts, notices = summarize(tracked_paths())
    except (OSError, subprocess.CalledProcessError):
        print("Could not inventory tracked vendored files.", file=sys.stderr)
        return 2
    print("Vendor area | tracked files | license-named files")
    for group in (*GROUPS, "ThirdParty/other", "Other scoped vendored paths"):
        print(f"{group} | {counts[group]} | {notices[group]}")
    print("Scope: current tracked paths only; names do not establish redistribution rights.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
