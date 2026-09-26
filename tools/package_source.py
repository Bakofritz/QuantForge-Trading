"""Reproducible, exact-commit source archive. Never packages workspace extras."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import subprocess
import tarfile
import zipfile


def package(root, output):
    root, output = Path(root), Path(output)
    def git(*args):
        return subprocess.check_output(['git', '-C', str(root), *args])
    if git('status', '--porcelain', '--untracked-files=no').strip():
        raise ValueError('Tracked working files differ from HEAD; commit before packaging.')
    commit = git('rev-parse', 'HEAD').decode().strip()
    tree = git('rev-parse', 'HEAD^{tree}').decode().strip()
    entries = {}
    with tarfile.open(fileobj=io.BytesIO(git('archive', '--format=tar', 'HEAD'))) as archive:
        for member in archive:
            if member.isdir():
                continue
            if not member.isfile():
                raise ValueError('Unsupported source entry: ' + member.name)
            if member.name == 'SOURCE_MANIFEST.json':
                raise ValueError('Reserved archive manifest path.')
            entries[member.name] = (archive.extractfile(member).read(), member.mode)
    manifest = {'schema': 1, 'commit': commit, 'tree': tree,
                'files': {name: hashlib.sha256(data).hexdigest()
                          for name, (data, _) in sorted(entries.items())}}
    entries['SOURCE_MANIFEST.json'] = ((json.dumps(manifest, sort_keys=True, indent=2) + '\n').encode(), 0o644)
    output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(output, 'w', compression=zipfile.ZIP_STORED) as archive:
        for name, (data, mode) in sorted(entries.items()):
            info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
            info.create_system = 3
            info.external_attr = (0o100000 | mode) << 16
            archive.writestr(info, data)
    digest = hashlib.sha256(output.read_bytes()).hexdigest()
    output.with_suffix(output.suffix + '.sha256').write_text(digest + '  ' + output.name + '\n')
    return digest


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    print(package(Path(__file__).resolve().parents[1], args.output))
