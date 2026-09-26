#!/usr/bin/env python3
"""AutoMellon Build Controller.

Executes only command IDs declared in the repository manifest and mapped in this
source file. It never executes arbitrary command strings from manifests, users,
or agents.
"""
from __future__ import annotations
import argparse, hashlib, json, os, pathlib, subprocess, sys, time
from datetime import datetime, timezone

ROOT = pathlib.Path(__file__).resolve().parents[1]
RESULT = ROOT / "AUTOMELLON_RESULT.json"
LOG_DIR = ROOT / "artifacts" / "automellon"
ALLOWED_BRANCH_PREFIXES = ("master/v30.00-phase5-production-hardening", "automellon/")
FORBIDDEN_BRANCHES = {"main", "master"}
COMMANDS = {
    "git_status": ["git", "status", "--porcelain=v1"],
    "static_validate": [sys.executable, "tools/validate_repository.py"],
    "package_source_test": [sys.executable, "tools/test_package_source.py"],
    "dotnet_info": ["dotnet", "--info"],
    "dotnet_restore": ["dotnet", "restore"],
    "dotnet_test": ["dotnet", "test", "--configuration", "Release", "--no-restore"],
}
MAX_TAIL_LINES = 80

def run(argv):
    started=time.time()
    p=subprocess.run(argv,cwd=ROOT,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
    lines=p.stdout.splitlines()
    return {"argv":argv,"exit_code":p.returncode,"duration_seconds":round(time.time()-started,3),
            "tail":lines[-MAX_TAIL_LINES:]}

def git(*args):
    return subprocess.check_output(["git",*args],cwd=ROOT,text=True).strip()

def fingerprint(path):
    h=hashlib.sha256()
    with open(path,"rb") as f:
        for chunk in iter(lambda:f.read(1024*1024),b""): h.update(chunk)
    return h.hexdigest()

def fail(reason, result, code=2):
    result["status"]="blocked"; result["block_reason"]=reason
    RESULT.write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print(json.dumps(result,indent=2)); return code

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--manifest",default="automellon/BUILD_MANIFEST.json")
    ap.add_argument("--phase",default=None)
    args=ap.parse_args()
    manifest_path=ROOT/args.manifest
    manifest=json.loads(manifest_path.read_text(encoding="utf-8"))
    branch=git("branch","--show-current"); commit=git("rev-parse","HEAD")
    result={"schema":"automellon-result/v1","generated_at":datetime.now(timezone.utc).isoformat(),
            "manifest":args.manifest,"manifest_sha256":fingerprint(manifest_path),
            "branch":branch,"commit":commit,"status":"running","phases":[]}
    authority=manifest["authority"]
    if branch in FORBIDDEN_BRANCHES:
        return fail("protected branch execution refused",result)
    if not any(branch==p or branch.startswith(p) for p in ALLOWED_BRANCH_PREFIXES):
        return fail("branch outside controller allowlist",result)
    if authority.get("live_trading") or authority.get("broker_orders") or authority.get("merge_protected_main"):
        return fail("manifest requests forbidden authority",result)
    LOG_DIR.mkdir(parents=True,exist_ok=True)
    phases=manifest["phases"]
    if args.phase:
        phases=[p for p in phases if p["id"]==args.phase]
        if not phases: return fail("requested phase not declared",result)
    for phase in phases:
        pr={"id":phase["id"],"status":"running","commands":[],"stable_checkpoint":bool(phase.get("stable_checkpoint"))}
        result["phases"].append(pr)
        for cid in phase["commands"]:
            if cid not in COMMANDS: return fail(f"unknown command id: {cid}",result)
            cr=run(COMMANDS[cid]); cr["id"]=cid; pr["commands"].append(cr)
            (LOG_DIR/f"{phase['id']}-{cid}.log").write_text("\n".join(cr["tail"])+"\n",encoding="utf-8")
            if cr["exit_code"]!=0:
                pr["status"]="failed"
                return fail(f"{phase['id']}:{cid} exit {cr['exit_code']}",result,cr["exit_code"] or 1)
        pr["status"]="passed"
        if pr["stable_checkpoint"]:
            result["checkpoint"]=phase["id"]
    result["status"]="passed"
    RESULT.write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print(json.dumps(result,indent=2)); return 0

if __name__=="__main__":
    raise SystemExit(main())
