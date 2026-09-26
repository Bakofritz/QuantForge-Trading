#!/usr/bin/env python3
"""Verify that an Android APK/build manifest belongs to an exact QuantForge source archive."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import zipfile


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            h.update(block)
    return h.hexdigest()


def source_identity(source_zip: Path) -> tuple[str, str]:
    with zipfile.ZipFile(source_zip, 'r') as archive:
        names = archive.namelist()
        if names.count('SOURCE_MANIFEST.json') != 1:
            raise ValueError('Source archive must contain exactly one SOURCE_MANIFEST.json.')
        manifest = json.loads(archive.read('SOURCE_MANIFEST.json'))
    commit = manifest.get('commit')
    tree = manifest.get('tree')
    if not isinstance(commit, str) or not isinstance(tree, str) or not commit or not tree:
        raise ValueError('Source manifest lacks commit/tree identity.')
    return commit, tree


def verify(source_zip: Path, apk: Path, build_manifest: Path) -> dict:
    commit, tree = source_identity(source_zip)
    metadata = json.loads(build_manifest.read_text(encoding='utf-8'))
    if metadata.get('git_commit') != commit or metadata.get('git_tree') != tree:
        raise ValueError('APK build commit/tree does not match source archive.')
    if metadata.get('apk_file') != apk.name:
        raise ValueError('APK filename does not match build manifest.')
    digest = sha256(apk)
    if metadata.get('apk_sha256') != digest:
        raise ValueError('APK SHA-256 does not match build manifest.')
    if metadata.get('apk_bytes') != apk.stat().st_size:
        raise ValueError('APK byte count does not match build manifest.')
    if metadata.get('target_framework') != 'net10.0-android':
        raise ValueError('Unexpected Android target framework.')
    return {
        'source_commit': commit,
        'source_tree': tree,
        'source_sha256': sha256(source_zip),
        'apk_sha256': digest,
        'apk_bytes': apk.stat().st_size,
        'verified': True,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-zip', required=True)
    parser.add_argument('--apk', required=True)
    parser.add_argument('--build-manifest', required=True)
    args = parser.parse_args()
    result = verify(Path(args.source_zip), Path(args.apk), Path(args.build_manifest))
    print(json.dumps(result, indent=2, sort_keys=True))
    return 0

if __name__ == '__main__':
    raise SystemExit(main())
