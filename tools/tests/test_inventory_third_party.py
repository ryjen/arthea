import importlib.util
import hashlib
import os
import subprocess
import tempfile
from pathlib import Path
import unittest

source = Path(__file__).resolve().parents[1] / "inventory_third_party.py"
spec = importlib.util.spec_from_file_location("vendor_inventory", source)
inventory = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inventory)

class ThirdPartyInventoryTests(unittest.TestCase):
    def test_exact_group_boundary(self):
        self.assertEqual("ThirdParty/Jint", inventory.group_for("ThirdParty/Jint/engine.dll"))
        self.assertEqual("ThirdParty/other", inventory.group_for("ThirdParty/JintFake/engine.dll"))
        self.assertIsNone(inventory.group_for("ArtheaServer/Source.cs"))
        self.assertEqual("ThirdParty/NLog.dll", inventory.group_for("ThirdParty/NLog.dll"))

    def test_counts_and_filename_evidence_are_distinct(self):
        counts, notices = inventory.summarize([
            "ThirdParty/Thought-Net-Telnet/LICENSE.txt",
            "ThirdParty/Thought-Net-Telnet/telnet.dll",
            "ThirdParty/Jint/lib.dll",
            "ThirdParty/Jint/COPYING",
            "Other/File.txt",
        ])
        self.assertEqual(2, counts["ThirdParty/Thought-Net-Telnet"])
        self.assertEqual(1, notices["ThirdParty/Thought-Net-Telnet"])
        self.assertEqual(2, counts["ThirdParty/Jint"])
        self.assertEqual(1, notices["ThirdParty/Jint"])
        self.assertNotIn("Other", counts)

    def test_exact_committed_bytes_not_modified_worktree(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            vendor = root / "ThirdParty" / "Jint"
            vendor.mkdir(parents=True)
            original = b"sample-vendor-bytes\x00\xff"
            (vendor / "Jint.dll").write_bytes(original)
            other = root / "Unrelated"
            other.mkdir()
            (other / "private.txt").write_text("excluded")
            subprocess.run(["git", "-C", str(root), "add", "."], check=True)
            subprocess.run(["git", "-C", str(root),
                "-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid",
                "commit", "-qm", "fixture"], check=True)
            (vendor / "Jint.dll").write_bytes(b"changed in worktree")
            rows = inventory.committed_fingerprints(root)
            self.assertEqual(["ThirdParty/Jint/Jint.dll"], [x["path"] for x in rows])
            self.assertEqual(hashlib.sha256(original).hexdigest(), rows[0]["sha256"])
            self.assertEqual(len(original), rows[0]["bytes"])
            self.assertEqual("ThirdParty/Jint", rows[0]["group"])

    def test_symlinks_are_not_followed(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            vendor = root / "ThirdParty" / "Jint"
            vendor.mkdir(parents=True)
            try:
                (vendor / "outside").symlink_to("../../untracked-secret")
            except (OSError, NotImplementedError):
                self.skipTest("Symbolic links unavailable")
            subprocess.run(["git", "-C", str(root), "add", "."], check=True)
            subprocess.run(["git", "-C", str(root),
                "-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid",
                "commit", "-qm", "symlink fixture"], check=True)
            self.assertEqual([], inventory.committed_fingerprints(root))

    def test_license_filename_is_not_coverage(self):
        self.assertTrue(inventory.license_named("ThirdParty/Thought-Net-Telnet/LGPL.txt"))
        self.assertTrue(inventory.license_named("ThirdParty/Jint/COPYING"))
        self.assertFalse(inventory.license_named("ThirdParty/Jint/engine.dll"))

if __name__ == "__main__":
    unittest.main()
