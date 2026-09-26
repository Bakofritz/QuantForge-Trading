import importlib.util
from pathlib import Path
import tempfile
import unittest

MODULE_PATH = Path(__file__).with_name("build_android_apk.py")
spec = importlib.util.spec_from_file_location("build_android_apk", MODULE_PATH)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)

class AndroidBuildToolTests(unittest.TestCase):
    def test_sha256_is_deterministic(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "x.apk"
            path.write_bytes(b"quantforge")
            self.assertEqual(module.sha256(path), module.sha256(path))
            self.assertEqual(len(module.sha256(path)), 64)

    def test_readiness_requires_exact_project_and_native_tools(self):
        state = module.readiness()
        self.assertTrue(state["project_exists"])
        self.assertIn("ready", state)
        self.assertIsInstance(state["ready"], bool)

if __name__ == "__main__":
    unittest.main()
