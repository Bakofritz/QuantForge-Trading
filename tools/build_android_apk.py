#!/usr/bin/env python3
"""Build and provenance-check the exact QuantForge Android APK from the current Git HEAD."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "src" / "QuantForge.App" / "QuantForge.App.csproj"


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def run(*args: str, cwd: Path = ROOT) -> str:
    return subprocess.check_output(args, cwd=cwd, text=True, stderr=subprocess.STDOUT)


def require_tool(name: str) -> str:
    path = shutil.which(name)
    if not path:
        raise RuntimeError(f"Required tool unavailable: {name}")
    return path


def git_identity() -> tuple[str, str]:
    if run("git", "status", "--porcelain", "--untracked-files=no").strip():
        raise RuntimeError("Tracked working files differ from HEAD; commit before APK build.")
    return run("git", "rev-parse", "HEAD").strip(), run("git", "rev-parse", "HEAD^{tree}").strip()


def android_sdk_root() -> Path:
    raw = os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME")
    if not raw:
        raise RuntimeError("ANDROID_SDK_ROOT/ANDROID_HOME is not configured.")
    root = Path(raw)
    if not root.is_dir():
        raise RuntimeError(f"Android SDK directory does not exist: {root}")
    return root


def build(output_dir: Path, configuration: str = "Release") -> dict:
    require_tool("git")
    dotnet = require_tool("dotnet")
    sdk = android_sdk_root()
    commit, tree = git_identity()

    output_dir.mkdir(parents=True, exist_ok=True)
    publish_dir = output_dir / "publish"
    if publish_dir.exists():
        shutil.rmtree(publish_dir)

    command = [
        dotnet, "publish", str(PROJECT),
        "-f", "net10.0-android",
        "-c", configuration,
        "-p:AndroidPackageFormat=apk",
        "-p:EmbedAssembliesIntoApk=true",
        "--output", str(publish_dir),
    ]
    completed = subprocess.run(command, cwd=ROOT, text=True, stdout=subprocess.PIPE,
                               stderr=subprocess.STDOUT, check=False)
    (output_dir / "android-build.log").write_text(completed.stdout, encoding="utf-8")
    if completed.returncode != 0:
        raise RuntimeError(f"Android publish failed ({completed.returncode}); see android-build.log")

    apks = sorted(publish_dir.glob("*.apk"))
    if len(apks) != 1:
        raise RuntimeError(f"Expected exactly one APK, found {len(apks)} in {publish_dir}")

    apk = apks[0]
    final = output_dir / f"QuantForge-{commit[:12]}-android.apk"
    shutil.copy2(apk, final)
    digest = sha256(final)
    metadata = {
        "schema": 1,
        "git_commit": commit,
        "git_tree": tree,
        "configuration": configuration,
        "target_framework": "net10.0-android",
        "android_sdk_root": str(sdk),
        "apk_file": final.name,
        "apk_bytes": final.stat().st_size,
        "apk_sha256": digest,
        "build_command": command,
        "authority": "simulation/read-only research; no live/order authority",
    }
    (output_dir / "APK_BUILD_MANIFEST.json").write_text(json.dumps(metadata, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    (output_dir / (final.name + ".sha256")).write_text(f"{digest}  {final.name}\n", encoding="utf-8")
    return metadata


def readiness() -> dict:
    dotnet = shutil.which("dotnet")
    sdk_raw = os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME")
    sdk = Path(sdk_raw) if sdk_raw else None
    return {
        "dotnet": dotnet,
        "android_sdk_root": str(sdk) if sdk else None,
        "android_sdk_exists": bool(sdk and sdk.is_dir()),
        "project_exists": PROJECT.is_file(),
        "ready": bool(dotnet and sdk and sdk.is_dir() and PROJECT.is_file()),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-dir", default=str(ROOT / "artifacts" / "android"))
    parser.add_argument("--check", action="store_true", help="report prerequisites without building")
    args = parser.parse_args()
    if args.check:
        state = readiness()
        print(json.dumps(state, indent=2, sort_keys=True))
        return 0 if state["ready"] else 2
    try:
        metadata = build(Path(args.output_dir))
        print(json.dumps(metadata, indent=2, sort_keys=True))
        return 0
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 2

if __name__ == "__main__":
    raise SystemExit(main())
