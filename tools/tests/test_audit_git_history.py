import importlib.util
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

path = Path(__file__).resolve().parents[1] / "audit_git_history.py"
spec = importlib.util.spec_from_file_location("audit_git_history", path)
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)

def git(root, *args):
    subprocess.run(["git", "-C", str(root), *args], check=True,
                   stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                   env={**os.environ, "GIT_CONFIG_NOSYSTEM": "1",
                        "GIT_CONFIG_GLOBAL": os.devnull})

def commit(root, name):
    git(root, "add", ".")
    git(root, "-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid",
        "commit", "-qm", name)

class HistoricalAuditTests(unittest.TestCase):
    def test_old_revision_detected_without_returning_secret(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            git(root, "init", "-q")
            # Construct synthetic data without embedding a credential-shaped
            # URL directly in this test's committed source file.
            value = "mongodb://" + "fixture" + ":" + "test-token" + "@host.invalid/test"
            (root / "app.config").write_text(value)
            commit(root, "fixture with old connection")
            (root / "app.config").write_text("safe config")
            commit(root, "clean current configuration")
            result = audit.audit(root)
            self.assertEqual(1, result["authenticated_url"])
            self.assertGreaterEqual(result["checked_text_blobs"], 2)
            self.assertNotIn("test-token", str(result))

    def test_clean_history(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            git(root, "init", "-q")
            (root / "readme.txt").write_text("hello")
            commit(root, "normal revision")
            result = audit.audit(root)
            self.assertEqual(0, result["authenticated_url"])
            self.assertEqual(0, result["private_key_marker"])

    def test_classifies_without_revealing_matching_bytes(self):
        candidate = "Pwd" + "=" + "a-fake-secret"
        matching = audit.PATTERNS["password_assignment"].search(candidate.encode())
        self.assertIsNotNone(matching)
        self.assertNotIn(candidate, str({"password_assignment": 1}))

if __name__ == "__main__":
    unittest.main()
