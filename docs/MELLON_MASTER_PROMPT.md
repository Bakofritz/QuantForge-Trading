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


## v30.02 — Bounded, explicit research-manifest inspection

- Continued after cross-chat continuity retrieval found no newer build or superseding approval rule; actual GitHub PR #9 still pointed to v30.01 `582511704f56e3cd86de46fd29fa8d48ee3cc4fc`. Its push `36096150473` and PR `36096154415` workflows passed all four jobs with 111 core tests. Main remains the protected Phase 4 baseline.
- Active branch remains `master/v30.00-phase5-production-hardening`; approved Phase 5 scope only. Intended commit: `v30.02: inspect bounded manifests without granting research admission`.
- Exposed parser defects: absent authority defaulted to enum zero; duplicate properties were accepted with last-value semantics; undefined authority and unsupported manifest versions could pass the manifest codec. The strict version-1 reader now requires all ten exact property names once, rejects unknown fields/versions/domains, preserves explicit LiveAccount rejection, bounds identity lengths to 512 characters, and limits JSON input to 64 KiB and depth 8.
- Added asynchronous stream inspection with a limit-plus-one-byte read budget, strict UTF-8 (optional BOM), SHA-256 of exact original bytes, explicit Inspected/Invalid/Unavailable/Cancelled results, and caller-owned stream lifetime. No filesystem path opening, writes, script execution, dataset admission or strategy admission occurs in this reader.
- Added a real user action to the MAUI shell: Inspect research manifest. The native picker uses OpenReadAsync (including Android content-URI providers), disables concurrent clicks, clears old inspection text, handles cancellation/provider failures, and reenables retry. Read cancellation uses a cooperative 30-second token; this is not a hard deadline for a noncooperative provider or picker opening.
- Inspection is metadata validation only. It does not call ApplySummary, run a backtest, execute scripts, admit the referenced data or strategy, change settings, or grant live authority. The research session remains governed by its existing independent gates.
- Added twenty regressions: missing/duplicate/unknown properties and authority; version/identity validation; exact-byte fingerprints with optional BOM and chunked reads; bounded oversized input; cancellation; provider failure/retry; corrupt/empty input; strict UTF-8; closed stream; undefined authority serialization.
- Local static and packaging checks passed. Exact-head native CI is pending at authoring; v30.02 remains a candidate until its own checks complete. No new failed revision known at authoring; existing failed history remains separate.
- Phase 5 approximately 25% planning estimate, subject to validation. Phases 1–4 retain their historical scoped completion. Overall usable/full progress is not audited against the complete application; inherited 99%/97% estimates do not certify product readiness.
- Hangups: actual picker/device runtime untested here; market-data import, persistent configuration/recovery, full import-to-research wiring, native distribution and sunflower UI remain unfinished. No supported NT8/Pine translator or complete backtest UI is implied.
- Next: real market-data byte parsing/admission/provenance and safe recovery before connecting user research execution; validate native picker and startup on Windows/Android. Contribution reminders remain deferred under existing code words; no request for large data or contributor activation now.
- Live environment remains research/simulation only, no broker credentials/connectivity/orders or protected-setting mutation. No phase transition, merge or final stable promotion authorized. Ordinary correction remains within existing Phase 5 approval; no new approval queue.
- Packaging retains exact-source archive, checksums, cumulative docs, full Mellon rules, PDF manual, UI reference, validation evidence and handoff. Recent complete CI runs took roughly eight to ten minutes; no justified whole-phase ETA.
- API implementation reference: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker?tabs=android (read 2026-09-25). UI uses provider streams, not FullPath file opening.


## v30.02a — Native picker resource-scope clarification

- Follow-up source review of the pinned .NET MAUI 10.0.20 implementation found that Android FilePicker uses ACTION_OPEN_DOCUMENT and may call EnsurePhysicalPath/CacheContentFile, which copies selected content to the app cache before returning it. The core reader's byte budget starts after that operation.
- Corrected the user manual's overly broad no-file-write statement: QuantForge does not modify the selected original, execute referenced scripts/paths, write application settings or grant admission, but native picker caching may occur. No new filesystem permission or live authority is introduced by this clarification.
- Known release-readiness limitation: selected-file caching, oversized provider selections, cache cleanup and noncooperative provider cancellation require device-level validation and potentially a specialized picker adapter before claiming a resource-bounded end-to-end import path. The current 64 KiB/strict-schema guarantees apply to ResearchManifestReader only.
- Reviewed exact upstream source: https://github.com/dotnet/maui/blob/10.0.20/src/Essentials/src/FilePicker/FilePicker.android.cs and https://github.com/dotnet/maui/blob/10.0.20/src/Essentials/src/FileSystem/FileSystemUtils.android.cs.
- Baseline candidate: f5db33ede05e0e0a599da47d6842b365bc10849a. Its core build/test/package passed with 131 tests; platform checks were still running when this clarification was authored. This is an evidence-driven documentation correction, not a failed compiler/test revision or a stable-release promotion.
- Phase 5 remains approximately 25% planning estimate. No additional feature progress is claimed for this clarification. Revalidate the exact new head and preserve all prior safety/approval/packaging rules.


## 2026-09-25 directive — usable APK delivery and controlled automation roadmap

User instruction: "Please log recommended suggestions to memory and incorporate into build phase as you see fit. Only export apk to me when in complete usable state. Mellon through entire phase if stability can be maintained."

This cumulative repository record is durable project memory. It does not claim a separate ChatGPT personal-memory write. Continue ordinary Phase 5 correction cycles automatically while exact-head blocking validation remains green; stop at an actual external validation, material-scope, protected-main merge, final-promotion or phase-transition boundary. Do not bypass human approval or imply work continues between chat turns.

APK delivery gate: do not send the user a shell-only, demo-only, compile-only, or unfinished test APK. Internal builds and validation may continue. Before user delivery, require an agreed usable research workflow: install/launch on Android; actual supported data import with identity and coverage checks; supported strategy admission and simulation; costs and causal fill semantics; readable chart/ledger/results; cancellation/error/retry and session recovery; documented limitations and reproducible packaging. Windows remains primary and Android is intended to retain near-full functionality. Full visual fidelity follows a validated runtime. No APK is authorized for delivery merely by CI success. Complete source ZIPs, full rules, cumulative documentation, PDF manual and handoff still accompany build reports.

Accepted sequencing and design corrections:
1. Phase 5: real market-data parsing/inspection, identity/provenance and coverage/reliability admission, usable import-to-simulation-to-results integration, configuration/recovery, native runtime/distribution validation, then target UI fidelity and exit review. Do not fabricate coverage or a live benchmark to unblock research.
2. Later research automation: wrap the existing QuantForge.Core in a versioned headless C# CLI; do not fork a second engine. Validated job schemas reference admitted data/strategy identities, bounded parameters, temporal splits and immutable per-run costs; diagnostics and exit codes remain explicit.
3. GitHub Actions orchestrates bounded batches with concurrency, duration, storage and spending caps. It is not unlimited compute. Artifact retention is finite: archive accepted evidence durably with hashes/manifests; version job definitions and evidence indices without filling source history with market data.
4. Roslyn syntax/semantic analysis is screening, not a security sandbox. Use a documented supported strategy subset/capability allowlist and isolated low-privilege execution with no secrets, restricted networking and CPU/memory/time limits. NT8 scripts require supported adapters/translations; generic C# compatibility is not runtime compatibility. Preserve original/sanitized provenance and user feature selection.
5. Isolate every run's ledger, dataset, strategy, contract specification, execution-policy version, parameters, costs, seeds and environment identity. The user's $0.95-per-side commission is explicit configuration and must not silently apply to every instrument/account.
6. Retain causal higher-timeframe availability and explicit eligible execution timing. Train/validation/untouched final-test partitions and walk-forward procedures must prevent repeated AI tuning against the final holdout. Retain failed trials as well as winners. Measured metrics do not prove profitability.
7. Overseer proposes quarantined jobs; a separate validated scheduler authorizes execution within quotas. An AI commit triggering compute is operational authority and must be controlled. No new workflow/settings/main mutation or live authority follows from this roadmap.
8. Profile before pooling or ref-struct optimization. Start with reproducible bounded search baselines, then compare adaptive search; do not promise zero allocation or require gradient search for nondifferentiable strategies.

Later research automation is not an implicit Phase 6 transition. Live-account authority, broker credentials/connectivity, live orders and protected-setting mutation remain disabled and separately governed. Existing data contribution and son's contributor workflow deferrals remain in effect.


## v30.05 current continuation state

Active phase: Phase 5, branch master/v30.00-phase5-production-hardening, draft PR #9. v30.04 baseline 4c9a192ac73c14f89795e536f5c542bc56208db4 passed both authoritative gates. Recheck v30.05's own exact head and tests before calling it validated. The shell can inspect manifests/minute exports, cancel and clear retained data, and compare two declared sources; these actions do not admit data, verify benchmark independence, run strategies or grant authority. Never convert source agreement to a live reliability score. User APK delivery remains withheld until the complete agreed usable workflow is runtime-validated. Phase completion, main merge and stable release remain unapproved; ordinary Phase 5 continuation is authorized.


### v30.05a correction state
v30.05 0be0726da8b5ec2d01386cf7294d4e7fc786c507 failed its hash-casing regression (176 passed, one failed). Preserve this failed classification. v30.05a accepts equivalent uppercase/lowercase SHA-256 text and retains the regression. Check the new exact head's gates before calling it validated. v30.04 remains the last all-green predecessor until then; main remains the Phase 4 protected baseline.


## v30.06 — Authorized diagnostic APK exception and local test recorder

The user explicitly approved early diagnostic APK testing on the S25 Ultra, then requested "Mellon. Initiate apk build and upload here so I can test." This supersedes the blanket no-early-APK rule for clearly labelled diagnostic test builds only. Ordinary release APKs still require complete usable workflow validation. No protected-main merge, final stable promotion, live authority or phase transition is authorized by this exception.

- Baseline: v30.05a 576a0ee932f621f241a485a8134a5e798453e295, push 36149455279 and PR 36149459213 green (177 tests). Continue current Phase 5 branch and draft PR #9.
- Diagnostic distribution is a separate application identity com.quantforge.diagnostic, title QuantForge Test, v0.30.6/code 3006. CI stages the signed APK, validates it with apksigner, records source SHA/signing certificate/checksum, and retains it for 14 days. Uses the build runner's development signing identity, not a production signing service. Future development certificates may differ; uninstall/reinstall may be needed after exporting diagnostics. Never include a signing private key in delivery.
- Explicit opt-in Start test session; local-only asynchronous bounded event recording (2000 attempts/session, 256 queued items, 2 MiB event file). Fixed action/outcome enums, UTC and monotonic elapsed timestamps, operation duration, managed-memory samples, device model/manufacturer/OS and display configuration. No device name/unique ID, credentials, selected file path/name/content, dataset or strategy bytes, typed contract text, raw exception message/stack or outside-app activity. Optional manually entered problem note (500 characters) is visibly included in export. No network uploader, broker connection, order authority or settings mutation is added.
- Persistent private event journal is best effort, flushed per event; OS kills, native crashes, ANRs and final queued writes are not guaranteed to be captured. Startup before opt-in and complete startup duration, CPU/battery/PSS profiling are not claimed. Missing stop marker/incomplete lines are disclosed in export. Session does not auto-resume after restart. Starting replaces the previous session; export it first. Stop drains the writer; clear removes local journal and the app's cached export, not copies already shared.
- Export creates a ZIP containing manifest.json, events.jsonl, recorder-status.json if available, and summary.txt; manifest binds event hash and build/device data. The Android share sheet is user-directed; opening it does not certify a successful upload. Restrict FileProvider to the sharing-root cache directory. Inspection and research admission remain separate; diagnostic events never become market/research evidence.
- Current APK functions: manifest and one-minute export inspection, source comparison, cancel/clear and diagnostic controls. Full strategy execution, charts/ledger workflow, persisted research recovery and sunflower navigation remain unfinished and are explicitly labelled. Device feedback is a validation contribution, not production-readiness approval.
- New regression coverage: allowed event schema, explicit notes only, stop/export, hash/manifest, flood limits, incomplete-tail recovery. Exact-head C#/native/signature/artifact validation required before delivery; pending at authoring. No new failed revision known yet. Preserve historical v30.05 failure separately.
- Planned commit: v30.06: ship opt-in diagnostic Android test build. Adds core journal/tests, app diagnostic panel, narrow Android share provider configuration; modifies MainPage/App, app identity/version, quality workflow and cumulative rules/manual/build log. No files removed. Phase 5 remains approximately 30% engineering estimate; full product readiness/whole-phase ETA unverified. Live authority disabled; ordinary current-phase correction and authorized diagnostic delivery need no repeat approval.


## v30.07 — Device-feedback correction: reject declared file-label mismatch

- User supplied two on-device diagnostic sessions from the PR build with synthetic merge identity b33f4abfca89cab4bafd9c44c1ea313a9c77bd19. GitHub verified parents: protected main 8dbd746a7e09f41f9651a3df8c6970cf0d7e5322 and v30.06c head 424c2af462c3d10a069df382a5d610563eeca472. Both source-head gates 36152798722/36152804482 succeeded. Do not mistake the PR merge-preview hash for an unknown branch or a protected-main merge.
- Both diagnostic event hashes/counts/sequences validated. No dropped events, storage errors or incomplete journal lines reported; sessions stopped normally. Exercised on-device operations included seven completed data inspections, ten comparisons, picker cancellation, lifecycle stop/resume and manual clear. This is scoped device evidence, not broad runtime or complete data-integrity certification. Raw uploaded logs and free-text notes are not committed to the public repository.
- Feedback correctly identified that declaration labels were not bound even to selected filenames. Added strict NT8 export filename consistency check before primary/reference stream opening: contract syntax, contract month/year and Last/Bid/Ask suffix must match declared labels. Wrong contract, rollover, series and unrecognized filenames reject the file. Original-file labels are still untrusted; a renamed file can forge them. No data contents identity, provenance, benchmark, session coverage or research admission is granted by this check. The UI now explicitly distinguishes matching labels from verified contents.
- Added FileLabelCheck diagnostic events with enum-only outcomes. Added DataProcessing/ComparisonProcessing timing beginning after picker return (includes stream opening and processing, not pure parser timing). Existing action durations retain end-to-end timing, including file selection. Memory figures in the uploaded sessions are managed heap samples only; no memory leak or performance certification is inferred.
- Avoid duplicate source SHA in assembly informational version. App becomes 0.30.7/code 3007, QuantForge Test, development signed. Existing test installs may need uninstall/reinstall if the CI signing certificate changes; export logs first.
- Adds 13 filename-consistency regressions. New exact-head core/native/APK signature gates pending at authoring. No new CI failure known. Earlier v30.05 failed candidate remains separately cataloged. Ordinary feedback correction and diagnostic APK delivery are authorized; production promotion/main merge/Phase 6 remain unapproved. Phase 5 approximately 30% engineering estimate; full product readiness/ETA not certified. Live authority disabled.


## v30.08 — APK2 feedback: file-first inspection and explicit rejection reasons

Baseline: v30.07 0895fa2e988ee339d913b918c2fba9b6e36a6848; push 36181934714 and PR 36181938631 green (193 core tests, 23 reflection-disabled JSON tests). Protected main remains 8dbd746a7e09f41f9651a3df8c6970cf0d7e5322. Continue master/v30.00-phase5-production-hardening and draft PR #9; no merge or Phase 6 authority.

APK2 feedback: two v30.07 sessions on SM-S938U / Android 16, 89 and 67 contiguous events; both hashes verified, no reported dropped events, storage failures or incomplete lines. Two minute inspections completed with post-picker durations 338/285 ms; three filename guards rejected. Nine manifest inspections were invalid and two were cancelled. Notes reported large-minute/day-file failures and requested automatic instrument/contract/timeframe handling. Existing logs lack precise error codes, so manifest-route confusion is plausible, not a proven size-limit diagnosis. No raw logs or user notes committed publicly.

Current correction: choose market-data TXT first; detect contract and Last/Bid/Ask from strict original NT8 filenames without manual typing. Label detection is untrusted convenience only, not contents identity, timeframe, timezone, provenance, coverage or research admission. Unknown labels fail; reference filenames must still match primary contract/expiry/series. One-minute UTC source scope remains explicit; daily/tick support and larger-file streaming are pending, not silently inferred or enabled.

Research metadata moved below market-data controls under Advanced, explicitly JSON-only / 64 KiB. Non-.json selections reject before opening the stream and direct minute TXT users to market-data inspection. The JSON contents validator remains authoritative; renaming cannot bypass it. Existing minute limits remain 8 MiB / 100,000 bars; no partial import. Friendly size/format messages distinguish metadata from market data. Diagnostic InspectionReason events use allowlisted enum values only, never filenames, paths, contents, typed labels or exception details. FileLabelDetected records outcome only.

Commit planned: v30.08: clarify file-first import and safe rejection diagnostics. Modified MainPage, filename rules, diagnostic enums, app version, CI APK name and cumulative docs; added ImportInspectionFeedback and 14 regression cases. No files removed. New exact-head validation pending at authoring. App 0.30.8/code3008; early diagnostic APK exception continues. Retest: choose a supported minute file without typing labels; compare a same-labelled source and reject a different contract/series; select TXT through advanced metadata and verify the corrective message; export diagnostics. New Start replaces old logs; export before reinstall/start if needed.

Phase 5 approximately 30% engineering estimate; overall usable/full-product percentage and whole-phase ETA remain unverified. Remaining blockers: data admission/provenance/coverage, supported executable strategy adapter, complete simulation/chart/ledger flow, persistent recovery, daily/larger data support and native runtime validation. Sunflower fidelity follows runtime. Live account/broker/orders disabled. Current contribution: Device Runtime Check after updated APK delivery; Market Data Samples via existing repository only when admission work needs them; Strategy Audit Pack and contributor workflow deferred. Ordinary Phase 5 correction and diagnostic delivery already authorized; no new approval queue. Main merge/final stable promotion/phase transition remain human-governed.


## v30.09 — Accepted device workflow and bounded streaming minute inspection

User confirmed v30.08 tests met expectations and invoked Mellon. Verified GitHub baseline 5e006506bf5baaa1913b8f6a8b5d496ecccd9a1d, push 36189020945 / PR 36189026144 green, 207 core tests plus 23 reflection-disabled JSON checks. Protected main unchanged at Phase 4 baseline; same Phase 5 branch/draft PR #9. Approval covers ordinary Phase 5 progression and diagnostic delivery, not main merge/stable promotion/Phase 6/live authority.

Device Runtime Check accepted for the exercised v30.08 flow: 99 contiguous events, matching SHA-256, normal stop, no reported drops/storage failures. Two file-first minute inspections and two comparisons completed; six comparison filename rejections and one wrong metadata file type met user expectations. Post-picker inspection 283/329 ms, comparison 361/369 ms. No raw logs/notes committed. This is scoped evidence, not full product or large-file certification.

v30.09 replaces whole-file byte/string allocation with a 64 KiB read buffer, bounded UTF-8 line buffer and incremental exact-byte SHA-256. Strict UTF-8, BOM, LF/CRLF, final-line handling, invariant decimal validation, chronology, no gap filling and no partial result publication remain. It reads the full valid file to EOF before returning success/hash. Cancellation, malformed tail, provider error, byte/bar overflow discard all parsed bars. Caller owns the stream. Malformed giant lines now reject early; the old zero-filled oversized fixture accordingly tests bounded early line rejection, while a new valid exact-byte boundary regression independently verifies the size limit and one-byte overflow.

Minute limits increase to 32 MiB / 500,000 bars, with bounded line length unchanged. Parsed bars are still retained for comparison; this is not constant-memory dataset storage, and phone peak/native memory is unverified. Read/provider timeout remains 30 seconds after stream opening. Day/tick formats, timezone or interval inference, live benchmark/provenance, admission and strategy execution are unchanged/unimplemented. Matching filename labels never establish contents identity.

Planned commit: v30.09: stream bounded minute files and retain complete-file integrity. Modified minute inspector, resource regression, UI/error limit descriptions, app version/APK workflow and cumulative documents; added StreamingMinuteInspectionTests (six cases: chunk/BOM/endings, exact 32 MiB/overflow, late encoding error and cancellation after parsed rows). No files removed. Exact-head CI/native/signature checks pending at authoring. App 0.30.9/code3009; preserve logs before reinstall when development signing certificate changes.

Next device contribution: Large Minute Check — use an original UTC minute file that exceeded the old 8 MiB or 100,000-bar limit, within 32 MiB / 500,000 bars; inspect, cancel/retry, compare if a matching source exists, then export diagnostic ZIP. Do not fabricate or rename data to bypass filename checks; no new bulk upload requested. Existing Market Data Samples repository remains available for later admission work; Strategy Audit Pack and son's contributor workflow remain deferred. Daily support is next data-format work after this bounded runtime validation.

Phase 5 approximately 32% engineering estimate for accepted small-file device flow and expanded streaming inspection, conditional on validation. Full usable/full-functionality percentage and whole-phase ETA remain unverified. Recent CI cycles are about ten minutes, not a full-phase estimate. Current hangups: larger-file phone validation, daily data, provenance/coverage/admission, executable strategy adapter and end-to-end simulation/chart/ledger/persistence. Sunflower fidelity follows runtime. Live environment: no broker, credentials, orders or live-account authority. Historical v30.05 failure remains separately cataloged; no v30.09 failure known at authoring. No new approval queue.


## v30.10 — Batch/ZIP market-data inspection, daily/tick formats and cross-validation

User supplied diagnostic export 5 and invoked Mellon, lifting the prior hold while v30.09 was tested. Verified GitHub v30.09 baseline 6ec5aaebd04d83437bdb0a29917126cbf37ec4aa and both green gates 36207889017/36207892609 (213 tests plus 23 reflection-disabled checks). Main remains protected at 8dbd746a7e09f41f9651a3df8c6970cf0d7e5322; continue the Phase 5 branch/draft PR #9. User explicitly requested next-build multi-file/ZIP, mixed MES/MNQ folders, misnamed-file handling, daily/tick support and cross-validation. The saved planning hold is superseded by this Mellon request; prior approval boundaries remain.

Diagnostic export 5: exact v30.09, Samsung SM-S938U/Android16, 160 contiguous events with matching hash, normal stop, no reported dropped events/storage failure/incomplete lines. Three minute inspections and two comparisons completed; eight filename rejections and one format rejection. The note identified added "minute" filename text. Logs do not contain source sizes/bar counts and therefore do not prove testing above previous limits. Raw uploaded logs and notes remain private.

New primary control selects multiple TXT/ZIP files via native picker; users may select mixed MES/MNQ files in the same folder or archive. ZIP subfolders are retained as member labels; no filesystem extraction or original renaming. Bounded batch: 64 selected files/result members and 64 entries per archive, 32 MiB per text file, 64 MiB total compressed ZIP input, 128 MiB total expanded text read, 500,000 rows per file, 1,000,000 retained rows total. ZIP staging uses bounded memory and parsed rows remain in memory; total/native memory is not profiled. Two-minute batch timeout begins after selection; provider opening cannot guarantee immediate cancellation. No recursive filesystem folder crawl or nested ZIP expansion. Unsupported members receive explicit rejection status.

Path traversal, absolute/drive/backslash paths, control characters, duplicate archive member paths and symbolic links reject the archive transaction. Successfully parsed members require exact decompressed length and CRC32 agreement; source/member SHA-256 retained. Cancellation, fatal archive/provider errors and global byte/file limits discard all batch results. Malformed individual text files publish no partial rows/hash; other independently valid files retain explicit status in a completed batch. Row retention budget can reject an individual file without publishing its partial data. Empty and unsupported archives are reported. Full valid text reads must reach EOF before publication.

Text shape determines Minute, Day, Tick or TickReplay. Strict UTF-8, bounded lines, invariant exact decimals, nonnegative values, OHLC relationships and integer volume enforced. Minute/daily timestamps strictly increase; tick ties retain source order and timestamps never decrease. Supported seconds and seven-digit subsecond tick stamps; replay rows preserve embedded Bid/Ask and Last volume. Replay content conflicts with a Bid/Ask label reject. No binary historical/replay database reader. UTC export convention is explicit; daily dates are labels, not inferred exchange-session boundaries. Source format references: https://ninjatrader.com/support/helpguides/nt8/importing.htm and https://ninjatrader.com/support/helpguides/nt8/exporting.htm (reviewed 2026-09-26 UTC). NinjaTrader documents export UTC/end-bar conventions and distinguishes daily, minute, tick and replay text layouts. Implementations remain QuantForge inspection contracts, not guaranteed provider/session equivalence.

Flexible filename token detection now accepts annotations such as MES 09-26.Minute.Last.txt in both batch and single-minute paths. It requires one unambiguous contract/expiry and price-series token. Rows without usable names are structurally inspected as unresolved identity. Exact byte matches can associate an unlabeled copy with a uniquely labelled peer; both remain unverified. Identical bytes with conflicting instrument/series labels flag every copy as IdentityConflict; none enter automatic cross-validation. Duplicate bytes are marked rather than merged. Price ranges never determine MES/MNQ. Arbitrary misnaming cannot be authoritatively corrected without independent identity evidence; manual/provenance-backed resolution remains unfinished. No source file is overwritten.

Cross-validation compares compatible nonduplicate, nonconflicted labelled sources. Same-format agreement covers observed timestamp ranges; tick ties compare source sequence. Tick/replay Last vs minute aggregates observed OHLCV into UTC [start,end) buckets labelled at end; an exact-boundary tick enters the next minute. Tick/replay comparison excludes unavailable quote fields; standalone Bid/Ask quote-stream semantics are never inferred from embedded replay quotes. Daily cross-format comparison requires an explicitly enabled UTC-calendar exploratory switch; it does not claim exchange-session validation. Minute bars ending midnight belong to the preceding UTC day; ticks at midnight to the new day. First/last buckets may be partial; absent ticks, market sessions and coverage completeness are not inferred. Counts show matching/conflicting and only-left/right observations. At most 128 eligible pairs, with omitted count disclosed. No reliability score, provenance or admission is granted by agreement.

Version 0.30.10/code3010; diagnostic heading now derives from app version, correcting stale v30.07 display. Fixed enum batch/cross-validation actions only in telemetry; no selected filenames, paths, labels, file content, raw exceptions or cross-results logged automatically. Reports on the user's screen show names/hashes; diagnostic export remains private and opt-in. Full backtesting/strategy execution/persistent market-data catalog remain unfinished. Windows native compile validation continues; no packaged Windows executable delivery is claimed.

Planned commit: v30.10: inspect mixed TXT ZIP batches and cross-validate observed market data. Adds MarketTextReader, MarketBatchInspection, MarketCrossValidation and regression coverage. Modifies filename rules, MainPage/DiagnosticPanel, diagnostic enum, app version, APK workflow and cumulative docs. No files removed. Exact-head core/native/signature/packaging checks pending at authoring. Ordinary requested Phase 5 work and early diagnostic delivery authorized; no main merge, final stable promotion, Phase 6 or live-account authority. Historical v30.05 failure stays separately cataloged.

Phase 5 approximately 38% engineering estimate, conditional on validation; overall usable/full-product readiness and whole-phase ETA not established. Current hangups: mixed-batch phone validation, independent instrument/provenance/session admission, authoritative daily alignment, persistent dataset storage and executable strategy/simulation/chart/ledger flow; sunflower fidelity follows runtime. Current contribution code: Mixed Import Check — select mixed TXT/ZIP samples already on device, inspect grouping/rejections/duplicates and cross-validation, then return diagnostic ZIP plus expectations. No bulk data upload requested. Market Data Samples repository remains available when admission work needs it; Strategy Audit Pack and son's workflow deferred. Live environment remains simulation/research-only with broker, credentials and orders disabled. Full source ZIP, PDF manual, complete rules, evidence and handoff required with delivery. No new approval queue.


## v30.10a — Native Switch compile correction

v30.10 source c1c8a21c0a1160065ef82d78652b3e135d806089 is FAILED / NON-STABLE: PR Quality Gate 36210000549 passed static checks, 235 core tests, 23 reflection-disabled JSON checks and deterministic packaging, but both Android and Windows failed CS0104 at MainPage.cs:14 (Switch ambiguous between MAUI Controls and System.Diagnostics). No v30.10 APK was produced or delivered. The latest fully validated diagnostic baseline remains v30.09, 6ec5aaebd04d83437bdb0a29917126cbf37ec4aa; protected main stays at 8dbd746a7e09f41f9651a3df8c6970cf0d7e5322.

Correction explicitly qualifies Microsoft.Maui.Controls.Switch. This changes type resolution only. Version remains 0.30.10/code3010 because no candidate APK was delivered; source SHA identifies v30.10a. Exact-head native builds, core tests, static checks, source packaging and APK checks must pass before delivery. Continued correction is authorized within Phase 5 on master/v30.00-phase5-production-hardening / draft PR #9. No merge, stable promotion, Phase 6 or live authority.

Files modified: src/QuantForge.App/MainPage.cs; docs/MASTER_BUILD.md; docs/MELLON_MASTER_PROMPT.md; docs/USER_MANUAL.md; releases/FAILED_ITERATIONS.md. No added or removed files. Commit: v30.10a: disambiguate MAUI Switch and record failed native candidate. Phase 5 remains approximately 38% engineering estimate, conditional on native and device validation; overall usable/full-product completion and full-phase ETA remain unverified. Mixed Import Check remains the next device contribution; strategy samples and contributor activation remain deferred. Current hangups remain mixed-batch device validation, trusted data/session admission, persistent storage and executable strategy/simulation/chart/ledger workflow. No new approval queue.
