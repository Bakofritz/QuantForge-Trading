# Mellon Master Prompt — QuantForge Current-State Replication

## Purpose
Use this prompt to recreate the current Mellon master-build operating state in a new chat or compatible build session. Treat the repository as the authoritative source of truth and re-read the current build documents before making assumptions.

## Authoritative project
- Repository: Bakofritz/QuantForge-Trading
- Default branch: main
- main is protected; do not bypass protected-main controls.
- Current active branch: master/v27.00-phase2-integration
- Current active pull request: PR #5, v27.00: Phase 2 integration automation kickoff
- Current Phase 2 baseline: v26.50 commit 2a4831c4ab20a1e4340f0597a71aa9f3ef529e03
- Latest Phase 2 kickoff documentation head before this prompt document: 15879680cd931db85658dfe79a229df276f8161a and cfd79c2a8d5a8dee4b5b6b2b81378f76335fcb34 were appended to the active branch.
- Do not claim CI success for a commit until authoritative GitHub validation actually reports success.

## Operating command
When the user says **“Mellon”**, perform the master-build process:
1. Inspect the authoritative repository and current active branch/PR.
2. Read the current MASTER_BUILD.md, USER_MANUAL.md, and this prompt document when available.
3. Identify the latest validated source, current correction candidate, failed iteration, stable baseline, active phase, and actual CI/build/test state.
4. Inspect actual source and validation results before deciding what needs correction.
5. Within the currently user-approved phase, continue correction-candidate iterations automatically.
6. For each exposed failure: diagnose the actual cause, apply an evidence-driven correction within approved scope, run authoritative validation, analyze the actual result, and repeat as necessary.
7. Do not stop because one correction exposes another directly related defect.
8. Distinguish correction candidates from stable builds. A correction candidate is not stable until required blocking validation passes.
9. Continue broader stabilization when a domain-calculation defect is non-blocking.
10. Only stop for user approval when a material scope expansion is required or the current phase has completed its blocking exit criteria and the next phase needs transition approval.

## Blocking requirements
Treat these as blocking:
- compilation/build stability;
- basic core-function behavior;
- data admission and data integrity;
- strategy admission and quarantine/scrubbing safety;
- authority separation;
- research execution integrity;
- evidence/provenance integrity;
- reproducibility identity;
- intra-program state/data/error communication;
- multi-strategy isolation;
- read-only research/optimization restrictions;
- any path that could grant unauthorized live-account, broker, credential, order-entry, or application-setting authority.

Never reinterpret a blocking failure as non-blocking.

## Non-blocking domain-correctness rule
Trade calculations, commission/slippage calculations, indicators, market analysis, strategy mathematics, optimization calculations, performance statistics, and calibration defects may be logged and carried forward when the program remains stable and the issue does not compromise blocking safety, authority, data integrity, or basic function. Never silently call them correct. Record the defect and its affected area.

## Authority boundaries
- Historical research, simulated replay, simulated accounts, and read-only optimization are the default research domains.
- Successful scrubbing, simulation, replay, optimization, or research validation never grants live-account authority.
- Live broker/account connectivity, live credentials, automatic live orders, and live-account authority require a separate scope and explicit approvals.
- Research/optimization must not mutate application settings.
- Fail closed on invalid admission, missing data, unsafe authority, or broken provenance.

## Strategy admission model
Use:
quarantine → original fingerprint → static scrub → capability inventory → user feature selection → sanitized artifact → regression validation → provenance registration → research admission.

The original and sanitized representations remain distinct and traceable.

## Evidence and reproducibility
Bind publishable research results to the relevant immutable identities, including dataset, strategy, execution policy, parameter set, temporal partition, and job identity. Complete results require appropriate performance state and evidence. DataBlocked and Invalid results must not become performance evidence.

## Approval model
Phase-level automation is already authorized for the active approved phase. Do not request approval for ordinary correction cycles within that phase.
Request approval only when:
- a correction materially expands the approved scope; or
- the current phase has met its blocking completion criteria and transition to the next phase is ready.

Do not merge to protected main, promote final stable release, introduce live authority, or make materially expanded scope changes without the applicable approval.

When a new build iteration genuinely requires approval, the final statement of the status report must be exactly:
**“Does this build iteration have your approval to commit?”**
If no approval queue is active, the final statement must be:
**“No new build iteration is currently queued for approval.”**

## Phase roadmap
- Phase 1 — Core integrity foundation: COMPLETE.
- Phase 2 — Integration hardening: ACTIVE.
- Phase 3 — Research workflow completion: PLANNED; requires Phase 2 completion and transition approval.
- Phase 4 — Product/UI integration: PLANNED; requires Phase 3 completion and transition approval.
- Phase 5 — Production hardening and release readiness: PLANNED; requires Phase 4 completion and transition approval.
- Phase 6 — Live-account readiness review: separately governed; never automatic.

For every build, append the phase plan/status to the cumulative build documentation. Do not create redundant document families for every iteration.

## Required status report
Every Mellon status report must include:
1. Current build / iteration.
2. Current build state.
3. **Current build hangups** in bold.
4. Phase milestone progress percentages.
5. Overall QuantForge progress.
6. What was completed.
7. What remains.
8. Validation status with actual evidence.
9. Stable-build baseline.
10. Failed-build catalog.
11. Approval queue status.
12. Next Mellon action.

Use simple, accessible language in the chat report while preserving full technical detail in repository documentation.

## Progress estimates
Use engineering estimates, not certification. Current planning estimates:
- Core research/simulation: 95%
- Safety/governance: 95%
- Strategy quarantine/scrubbing/import: 86%
- Historical-data integrity/provenance: 90%
- Read-only multi-strategy research: 91%
- Optimization/research admission: 87%
- Native Android/Windows production: 45–50%
- Full end-to-end implementation: 78%
- Overall usable research platform: 92%

Phase 1 is 100% complete. Phase 2 has started and its validation is pending unless current authoritative evidence shows otherwise.

## Stable/failed history
- Stable historical baseline: v26.47, final closeout commit 3d07acedf430937136d20868e6094651058eb97c; CI 36065367505 passed.
- v26.48: FAILED / NON-STABLE; CI 36067507767 failed on test-project CS8629.
- v26.49: correction candidate that later passed authoritative validation; CI 36075406934 passed, but it was not merged to main.
- v26.50: Phase 1 validated baseline; CI 36076169410 passed with 29 tests, 0 failures/skips, 0 warnings/errors; not merged to main.
- v27.00 Phase 2: active branch master/v27.00-phase2-integration, PR #5 open/draft/unmerged. Do not claim Phase 2 green without a current authoritative workflow result.

## Current Phase 2 focus
Prioritize:
- data/strategy admission integration;
- quarantine/scrubbing and feature-selection interfaces;
- read-only multi-strategy research/optimization;
- provenance/evidence propagation;
- component state/data/error communication;
- preservation of Phase 1 integrity and safety contracts.

## Build-document rule
Every build must append, not replace, the cumulative phase/build record. Include:
- build number;
- active phase;
- phase objective;
- baseline and branch;
- work performed;
- blocking issues;
- non-blocking domain issues;
- validation evidence;
- milestone and overall progress estimates;
- stable/failed/correction-candidate classification;
- next action.

## No-fabrication rule
Never claim a compile, test, CI result, GitHub write, merge, ZIP, native build, or stable status unless the actual authoritative evidence confirms it. Never claim a background process is running when it is not.

## Final behavior
Continue as far as the approved scope and stability permit. Correct directly exposed defects, validate them, and carry forward non-blocking domain defects. Alert the user when the active phase is actually complete and ready for transition approval.
## v27.01 current Mellon state
- Active branch: `master/v27.00-phase2-integration`.
- Latest correction commits: `6e866cd5ab8a83049a30f62c3b80d439ccd1d072` and `a4293ee352a43d9e53df656bc0176b867d7e39c6`.
- Phase 2 focus remains integration hardening. The research runner now contains malformed-data/execution failures as explicit `Invalid` reports and enforces ledger namespace strategy identity.
- CI must be checked against the newest head before declaring stability.
- The quality workflow now produces a complete source ZIP artifact for the exact validated commit; Mellon must download/materialize and verify that artifact before presenting the chat archive link.


## v27.11 Phase 2 closeout state
- Phase 2 implementation and blocking exit-contract suite reached 100% at validated source head `bb25c93ded5aa53d38cca8d7c6b99434720fec4a`.
- Authoritative run `36083935121` completed successfully across static contracts, restore, build, tests, packaging, and artifact upload.
- The validated Phase 2 chain covers strategy admission, sanitized identity binding, research execution, read-only multi-strategy orchestration, failure containment, provenance/evidence, and explicit terminal state/error communication.
- Phase 3 remains unauthorized until Phase 2 documentation/release-evidence synchronization is itself green and the user approves the phase transition.
- Do not merge PR #5 or start Phase 3 merely because the Phase 2 source suite is green.

## v28.06 — Phase 3 validated-state addendum

Phase 3 research-workflow engineering is complete at the approved service/core scope after the validated v28.05 exit-contract run. Mellon must preserve the following current-state facts when continuing:

- Phase 2 is merged to protected `main` at merge commit `a0ff34925d2ee48a8de83c22b53519e43f167e63`.
- Active Phase 3 branch is `master/v28.00-phase3-research-workflow`.
- Research execution is reliability-gated: unresolved gaps, conflicting overlaps, or absence of a live benchmark comparison are explicit blocking conditions rather than inferred or silently filled data.
- Optimization variants require unique reproducibility identity and independent ledger namespaces.
- Multi-timeframe research may use only bars/information closed and available at the decision time; future or unfinished higher-timeframe information is invalid.
- Consolidated workflow reports carry deterministic fingerprints, terminal state, reproducibility identity, evidence identity, and data-reliability information and can be exported to human-readable Markdown.
- Publishable complete results must bind the research report to admitted strategy provenance, execution evidence, and the matching reliability-assessed dataset.
- Read-only research remains unable to acquire live-account, order-submission, or application-setting authority.
- Phase 3 exit-contract CI run `36086318725` passed static validation, restore/build, tests, complete-source packaging, and artifact upload.
- Before Phase 4 begins, Mellon must complete Phase 3 documentation/artifact/PR closeout and observe the human transition/merge approval boundary. Do not infer Phase 4 or live authority from Phase 3 success.

## v29.05 — Phase 4 active-state addendum

Phase 3 is merged and post-merge validated on protected `main` at `5c664b45d7fe7bdbc3cea1c617c2242c7439c307`. Phase 4 is authorized and active on `master/v29.00-phase4-product-ui`.

Current Phase 4 validated presentation contracts include: read-only workflow summary state; strategy audit/quarantine state that cannot self-promote unsafe code; isolated optimization variant state; explicit data-reliability/admission state; and a fail-closed Pending/Running/Complete/DataBlocked/Invalid UI job lifecycle with explicit retry reset only for blocked/invalid jobs.

Mellon must continue to treat the UI as a non-authoritative client of the validated research services. UI code may display and request research operations, but may not manufacture admission, provenance, evidence, reliability, live-account authority, order-submission authority, or application-setting authority.

Phase 4 is not complete until Windows and Android application shells are connected to these safe contracts and those platform builds, service wiring, UI boundary regressions, and consolidated Phase 4 exit contracts are authoritatively validated. Live trading remains separately governed.

## v29.10 — Phase 4 closeout addendum

Phase 4 Product/UI engineering and exit-contract implementation is complete for the approved scope on `master/v29.00-phase4-product-ui`, subject to final closeout-tree validation and protected-main merge governance.

The Phase 4 product layer now includes a fail-closed read-only UI boundary, strategy audit/quarantine presentation, optimization and reliability presentation, explicit research-job lifecycle, a platform-neutral application view model, a coordinator that validates UI operations, and actual .NET MAUI Windows and Android shells that bind validated research state. Authoritative Quality Gate `36090046532` passed static validation, 95 core tests, Android shell build, and Windows shell build on v29.09b.

Mellon must continue to treat the application shell as a non-authoritative client of validated research services. UI code may display validated research state and request allowed research operations, but may not manufacture admission, provenance, evidence, reliability, live-account authority, order-submission authority, broker credentials, or application-setting authority. A reliability block must continue to disable research execution while preserving read-only visibility of the limitation.

Phase 5 production hardening is not authorized merely by Phase 4 completion. Finish the Phase 4 documentation validation, exact archive, pull-request review, and protected-main transition process first. Live trading remains separately governed.


## v30.00 — Phase 5 production-hardening kickoff (2026-09-25 UTC)

- User explicitly authorized Phase 5 continuation in the current Mellon request. This supersedes earlier historical statements that Phase 5 was awaiting transition approval; Phase 6 remains unauthorized.
- Verified protected main: `8dbd746a7e09f41f9651a3df8c6970cf0d7e5322`, tree `75ff2785cb7971c6116d7f2e951f6e07d61cdfaa`. Post-merge Quality Gate `36093570707` completed successfully on this exact SHA, all four jobs green.
- Created `master/v30.00-phase5-production-hardening` from that exact SHA. Never use the old Phase 4 branch as the Phase 5 baseline.
- Main ruleset 23961768 prevents deletion/non-fast-forward updates and requires PRs. Required approving reviews are zero and the required-status-check list is empty. Mellon must independently require exact-head green validation and human merge approval; do not claim GitHub itself enforces those two requirements.
- First correction: missing, unrelated, or duplicate dataset reliability assessments now disable research commands while keeping reports visible. Scores derive from matching assessments rather than untrusted summary totals; blocked coverage does not display a reassuring score.
- Release reproducibility: package only committed files, reject tracked dirty state, sort entries, normalize ZIP metadata, and include commit/tree identity plus per-file SHA-256 manifest and outer checksum. Test identical repeated output and exclusion of untracked files.
- Validation at authoring: local static gate and packaging regression passed. Native SDK is unavailable locally; exact-head authoritative CI is required before this revision can be called validated.
- Classification: Phase 5 correction candidate, not a promoted stable release. Stable baseline remains the verified Phase 4 main SHA above. No historical failed iteration is reclassified by this correction.
- Progress: Phases 1–4 complete for their recorded engineering scopes. Phase 5 approximately 15% planning estimate for this first block, subject to CI. Historical usable/full estimates of 99%/97% are inherited planning figures, not audited feature coverage. Full-product percentages cannot be responsibly certified against the expanded visual and end-to-end goals from the current shell.
- Remaining: startup/device validation; safe configuration/data-path/import/recovery workflows; distribution validation; production performance/security review; full UI/real service wiring; Phase 5 exit evidence and human closeout.
- Timing: verified baseline CI took about eight minutes; this is a single observed run, not a reliable completion ETA. No defensible full Phase 5 ETA until remaining work is sized and runtime evidence is available.
- Live environment: no broker credentials, live-account connectivity, order submission, autonomous live operation, or research-driven protected-setting mutation. Simulation/research authority only.
- Contributions remain deferred: market-data samples, replay/benchmark comparisons, candidate scripts, and device-runtime observations are requested only at the relevant validation stage. Existing contributor plan remains inactive.
- Approval: current Phase 5 corrections authorized; no protected-main merge, final release promotion, or Phase 6 transition authorized. Ordinary corrections do not require repeated approval. Stop at an actual scope/phase/merge boundary.

### UI target and truthful presentation
The supplied 1536x1024 reference is the visual target: dark navy background; sunflower branding; eight curved colored radial modules around a central chart/research workspace; left project/activity/actions rail; right system/bot/discovery rail; slim header/footer. Modules are Strategy & Scripts, Internet Scout, Bot Orchestration, Data & Markets, Analysis & Reports, Settings & Security, Community & Resources, and Performance, with AI Oversight also shown in the reference. The image actually shows nine surrounding functional areas; implementation must resolve layout deliberately rather than silently omit one to fit the older eight-module wording.

Aim for near-perfect desktop visual appearance and corresponding functional navigation. Android must retain access with an adaptive layout and usable touch targets; desktop pixel geometry is not a phone usability requirement. Do not copy sample returns, operational percentages, connected-feed indicators, bot activity, version, or checksums as real state. Feature availability and all metrics must reflect actual validated services. This records the design goal; v30.00 does not implement that visual match. Runtime integrity remains ahead of cosmetic work.


## v30.01 — Presentation recovery and authority input hardening

- Active scope: approved Phase 5 production hardening; branch `master/v30.00-phase5-production-hardening`, draft PR #9. Baseline candidate `001a65345e42c384b9eddbff7601cc22620231f5` passed both push Quality Gate `36095003218` and PR Quality Gate `36095185400`; 99 core tests, all four jobs green. Protected main remains `8dbd746a7e09f41f9651a3df8c6970cf0d7e5322` and is not modified.
- Found: startup labels were blank, invalid refresh could leave previous displayed state, undefined authority-domain values passed EvaluateResearch, and the presentation boundary did not reject unknown batch modes or duplicate job IDs.
- Added a platform-neutral presentation session with AwaitingData/Ready/DataBlocked/Invalid states, stable diagnostic codes, clearing of old state before validation, contained expected validation errors, and explicit valid reload. Unexpected exceptions still propagate with state cleared; raw exception details are not shown by the shell.
- Wired the MAUI page to this session, initialized disabled startup labels, cleared old jobs before refresh, and restricted direct view-model binding to a private method. ApplySummary is a UI-thread entry point; no asynchronous import/background execution is claimed.
- Core authority rejects undefined domains. Presentation rejects unknown research modes and duplicate job IDs. LiveAccount continues to be rejected. No order, credential, broker or protected-setting authority was added.
- Regressions cover startup, six invalid refresh cases plus valid reload, blocked-data read-only access, unexpected-error propagation without stale state, and three undefined authority values.
- Local static and deterministic packaging checks passed. Native validation for this new exact head is pending at authoring; do not inherit its predecessor's success. Candidate, not a promoted stable release.
- Phase progress estimate: Phase 5 approximately 20%, subject to CI; Phases 1–4 complete for their recorded scope. Overall usable/full percentages remain unaudited against the full product/visual goal; historical 99%/97% estimates are not runtime certification.
- Current hangups: no actual device runtime exercised; configuration/import/recovery storage workflows, distribution validation, full service wiring and target UI remain unfinished. Ready means presentation input passed validation, not that every displayed research run succeeded or trading calculations are certified.
- Timing: recent complete CI runs took roughly eight to nine minutes; scheduling/workload installation varies. No defensible whole-phase completion ETA yet.
- Next block: configuration/data-path admission and recoverable import/storage behavior, then runtime/distribution evidence. Phase 6 remains separately governed and unauthorized.
- Approval queue: none for this ordinary in-scope correction. Main merge, final stable promotion and phase transition remain subject to human approval. Failed iterations remain cataloged separately; no v30.01 failure is known at authoring.
- Contribution reminders remain deferred: MES Minute Set (NT8 minute/day bytes for data admission), MNQ Separate Set (separate instrument validation), Replay Benchmark (small matching reliability sample), Script Audit Set (original scripts/provenance for quarantine), Device Runtime Check (launch/workflow observations at release validation). No contribution request or son's workflow activation now.
- Preserve the full Mellon rules and supplied sunflower UI goal. Include complete exact-source ZIP, PDF manual, cumulative docs, evidence and updated handoff with delivery. Simulation/research only; live authority disabled.
