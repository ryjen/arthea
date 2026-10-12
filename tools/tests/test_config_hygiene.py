import importlib.util
import unittest
from pathlib import Path

file = Path(__file__).resolve().parents[1] / "check_config_hygiene.py"
spec = importlib.util.spec_from_file_location("config_hygiene", file)
hygiene = importlib.util.module_from_spec(spec)
spec.loader.exec_module(hygiene)

class ConfigHygieneTests(unittest.TestCase):
    def test_clean(self):
        self.assertIsNone(hygiene.check_text("<configuration/>"))

    def test_connection_string_entry(self):
        self.assertIsNotNone(hygiene.check_text(
            '<configuration><connectionStrings><add name="test" connectionString="placeholder"/></connectionStrings></configuration>'
        ))

    def test_embedded_mongodb_password(self):
        msg = hygiene.check_text('<configuration><add value="mongodb://user:secret@host.invalid/db"/></configuration>')
        self.assertIsNotNone(msg)
        self.assertNotIn("secret", msg)

    def test_generic_password_assignment(self):
        self.assertIsNotNone(hygiene.check_text('<configuration><add value="Password=placeholder;"/></configuration>'))

    def test_invalid_xml(self):
        self.assertIsNotNone(hygiene.check_text("<configuration>"))

    def test_current_configs(self):
        self.assertEqual([], hygiene.check_tree())

if __name__ == "__main__":
    unittest.main()
