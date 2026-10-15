import importlib.util
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

    def test_license_filename_is_not_coverage(self):
        self.assertTrue(inventory.license_named("ThirdParty/Thought-Net-Telnet/LGPL.txt"))
        self.assertTrue(inventory.license_named("ThirdParty/Jint/COPYING"))
        self.assertFalse(inventory.license_named("ThirdParty/Jint/engine.dll"))

if __name__ == "__main__":
    unittest.main()
