import importlib.util
import hashlib
import json
from pathlib import Path
import tempfile
import unittest
import zipfile

MODULE_PATH = Path(__file__).with_name('verify_android_apk_bundle.py')
spec = importlib.util.spec_from_file_location('verify_android_apk_bundle', MODULE_PATH)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)

class AndroidBundleVerifierTests(unittest.TestCase):
    def make_fixture(self, root: Path):
        source = root/'source.zip'; apk=root/'QuantForge-abc-android.apk'; manifest=root/'APK_BUILD_MANIFEST.json'
        with zipfile.ZipFile(source,'w') as z:
            z.writestr('SOURCE_MANIFEST.json', json.dumps({'commit':'abc','tree':'tree1','files':{}}))
        apk.write_bytes(b'apkbytes')
        metadata={'git_commit':'abc','git_tree':'tree1','apk_file':apk.name,'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'apk_bytes':apk.stat().st_size,'target_framework':'net10.0-android'}
        manifest.write_text(json.dumps(metadata),encoding='utf-8')
        return source,apk,manifest

    def test_verify_accepts_exact_source_apk_pair(self):
        with tempfile.TemporaryDirectory() as d:
            source,apk,manifest=self.make_fixture(Path(d))
            self.assertTrue(module.verify(source,apk,manifest)['verified'])

    def test_verify_rejects_relabelled_apk(self):
        with tempfile.TemporaryDirectory() as d:
            source,apk,manifest=self.make_fixture(Path(d))
            apk.write_bytes(b'different')
            with self.assertRaises(ValueError): module.verify(source,apk,manifest)

if __name__=='__main__': unittest.main()
