import importlib.util
from pathlib import Path
import subprocess
import tempfile
import unittest

SOURCE = Path(__file__).resolve().parents[1] / "check_publication_hygiene.py"
SPEC = importlib.util.spec_from_file_location("publication_hygiene", SOURCE)
PUBLICATION = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PUBLICATION)

SCREENS = (
    "TITLE.ANS",
    "STORY1.ANS",
    "STORY2.ANS",
    "STORY3.ANS",
    "STORY4.ANS",
)


class PublicationHygieneTests(unittest.TestCase):
    def test_restricted_screen_names_under_exact_directory(self):
        directory = PUBLICATION.RESTRICTED_SCREEN_DIRECTORY
        files = [f"{directory}/{name}" for name in SCREENS]
        self.assertEqual(sorted(files), PUBLICATION.check_paths(files))

    def test_other_paths_and_non_screen_files_are_not_blocked(self):
        directory = PUBLICATION.RESTRICTED_SCREEN_DIRECTORY
        self.assertEqual([], PUBLICATION.check_paths([
            f"{directory}/ReadMe.txt",
            "Samples/OtherGame/Text Files/TITLE.ANS",
            f"{directory}Backup/TITLE.ANS",
            "ThirdParty/Thought-Terminals/ReadMe.txt",
        ]))
        self.assertTrue(PUBLICATION.restricted_screen(
            f"{directory}/another-screen.ans"
        ))

    def test_scans_committed_head_not_untracked_worktree(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            subprocess.run(["git", "init", "-q", str(root)], check=True)
            (root / "README.md").write_text("fixture")
            directory = root / PUBLICATION.RESTRICTED_SCREEN_DIRECTORY
            directory.mkdir(parents=True)
            restricted_file = directory / SCREENS[0]
            restricted_file.write_bytes(b"synthetic restricted-screen fixture")
            subprocess.run(["git", "-C", str(root), "add", "."], check=True)
            subprocess.run([
                "git", "-C", str(root),
                "-c", "user.name=Fixture",
                "-c", "user.email=fixture@example.invalid",
                "commit", "-qm", "synthetic fixture",
            ], check=True)
            self.assertEqual(
                [str(restricted_file.relative_to(root))],
                PUBLICATION.check_current_tree(root),
            )
            subprocess.run([
                "git", "-C", str(root), "rm", "-q",
                str(restricted_file.relative_to(root))
            ], check=True)
            subprocess.run([
                "git", "-C", str(root),
                "-c", "user.name=Fixture",
                "-c", "user.email=fixture@example.invalid",
                "commit", "-qm", "exclude fixture",
            ], check=True)
            self.assertEqual([], PUBLICATION.check_current_tree(root))


if __name__ == "__main__":
    unittest.main()
