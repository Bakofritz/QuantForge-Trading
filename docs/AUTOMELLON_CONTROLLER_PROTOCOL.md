# AutoMellon Build Controller Protocol v1.0

## Canonical source
GitHub repository `Bakofritz/QuantForge-Trading` is the canonical source, audit trail, CI source and artifact source for AutoMellon. Routine source ZIP/handoff uploads through chat are retired. Chat may still be used for exceptional recovery artifacts when GitHub itself is unavailable.

## Roles
**Controller:** deterministic executor. It follows `automellon/BUILD_MANIFEST.json`, executes only command IDs hard-coded in `tools/automellon_controller.py`, records evidence, and never invents remediation.

**AutoMellon orchestrator:** reviews GitHub state and controller/Actions evidence, diagnoses failures, edits source/protocol through authorized GitHub operations, and starts/retries bounded CI operations. It may not expand controller authority implicitly.

**Owner:** controls protected-main merge, stable promotion when owner approval is required, live-account/broker/order authority, credential changes, and changes to these authority boundaries.

## Closed-loop operation
1. Read exact branch HEAD and manifest.
2. Run controller phases in order.
3. Stop immediately on nonzero exit, unknown command, authority violation, or protected branch.
4. Emit `AUTOMELLON_RESULT.json` plus bounded per-command logs.
5. GitHub Actions uploads controller evidence.
6. On PASS, continue only to the next manifest phase/iteration.
7. On FAIL/BLOCKED, AutoMellon reads compact result first and full job log only when needed, makes a bounded source correction, and lets GitHub run the controller again.
8. Every 15 feature iterations, pause for a stable checkpoint report. The checkpoint is an engineering checkpoint, not a claim of release stability.
9. Native build success, device acceptance, release promotion, and live authority remain separate evidence/authority domains.

## Hard prohibitions
The controller must never: execute manifest-provided shell strings; use `eval` or shell interpolation; force-push; merge or write protected `main`; weaken tests/analyzers to manufacture PASS; fabricate evidence; enable live trading, broker orders, or credentials; relabel an artifact from another commit/version; or continue after a failed required gate.

## Command expansion
New command IDs require a reviewed source change to both the Python allowlist and manifest. Each command uses an argv array with `shell=False`. Commands that mutate Git history, releases, credentials, branch protection, or trading authority are not eligible for the autonomous allowlist.

## Error contract
`AUTOMELLON_RESULT.json` is the primary machine interface. A failure reports branch, commit, manifest hash, phase, command ID, exit code, duration, and a bounded output tail. Full logs remain GitHub Actions artifacts. AutoMellon should request full logs only when the compact evidence is insufficient.

## GitHub-only handoff
Repository source, manifests, protocol, CI evidence and build artifacts live in GitHub. A new AutoMellon session begins by reading this protocol, the build manifest, current branch HEAD, latest controller result/artifact, and current Phase documentation. No chat-local ZIP is required for normal continuation.
