import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
import zipfile
from package_source import package


class SourcePackageTests(unittest.TestCase):
    def test_exact_repeatable_source_and_dirty_rejection(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            def git(*args):
                subprocess.run(['git', '-C', directory, *args], check=True, capture_output=True)
            git('init')
            git('config', 'user.name', 'Package test')
            git('config', 'user.email', 'package-test@example.invalid')
            (root / 'source.txt').write_bytes(b'committed source\n')
            git('add', 'source.txt')
            git('commit', '-m', 'fixture')
            (root / 'untracked-secret.txt').write_text('must not be packaged')
            first = root / 'first.zip'
            second = root / 'second.zip'
            self.assertEqual(package(root, first), package(root, second))
            self.assertEqual(first.read_bytes(), second.read_bytes())
            with zipfile.ZipFile(first) as archive:
                self.assertEqual(set(archive.namelist()), {'source.txt', 'SOURCE_MANIFEST.json'})
                manifest = json.loads(archive.read('SOURCE_MANIFEST.json'))
                self.assertEqual(manifest['files']['source.txt'], hashlib.sha256(archive.read('source.txt')).hexdigest())
                self.assertEqual(len(manifest['commit']), 40)
            (root / 'source.txt').write_text('uncommitted change')
            with self.assertRaises(ValueError):
                package(root, root / 'dirty.zip')


if __name__ == '__main__':
    unittest.main()
