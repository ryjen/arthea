#!/usr/bin/env python3
"""Current application-config hygiene only; not a Git-history or release audit."""
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
CONFIGS = (
    Path("ArtheaServer/app.config"),
    Path("ArtheaEditor/app.config"),
    Path("ArtheaEngine.Tests/app.config"),
)

CREDENTIALS = (
    re.compile(r"mongodb(?:\+srv)?://[^/@\s:]+:[^/@\s]+@", re.IGNORECASE),
    re.compile(r"(?:password|pwd)\s*=\s*[^;\s\"']+", re.IGNORECASE),
)

def check_text(value):
    if any(pattern.search(value) for pattern in CREDENTIALS):
        return "credential-shaped setting"
    try:
        xml = ET.fromstring(value)
    except ET.ParseError:
        return "invalid XML"
    if xml.find(".//connectionStrings/add") is not None:
        return "source-controlled connection-string entry"
    return None

def check_tree(root=ROOT):
    problems = []
    for name in CONFIGS:
        try:
            finding = check_text((root / name).read_text(encoding="utf-8-sig"))
        except OSError:
            finding = "missing/unreadable config"
        if finding:
            problems.append((str(name), finding))
    return problems

if __name__ == "__main__":
    findings = check_tree()
    for path, finding in findings:
        print(f"{path}: {finding}", file=sys.stderr)
    if findings:
        sys.exit(1)
    print("Current app configs pass hygiene checks; historical commits NOT scanned.")
