# QuantForge Master Build

## Operating rule

This repository is maintained as a cumulative master build. Iterations are incremental and recorded here instead of producing redundant document families.

## v26.41 — Clean GitHub Directory Overhaul
- Converted the repository into explicit source, test, research, docs, tools, release, and CI boundaries.
- Preserved v26.35 artifacts as immutable release evidence.
- Added the native QuantForge.Core project boundary and fail-closed research contracts.
- Added architecture tests and a static repository gate.
- Native .NET compilation remains unverified locally because no .NET SDK is installed in the build environment.

## v26.42 — Data/Strategy Admission Hardening
- Added canonical research-manifest serialization and validation.
- Added explicit user-selectable strategy feature categories.
- Added immutable provenance-record contract connecting original and sanitized artifact fingerprints to a scrub report.
- Research manifests cannot represent LiveAccount authority.
- Research feature selection rejects order submission, application-setting mutation, and unreviewed process/native-library execution.
- Added regression tests for manifest round-trip, live-authority rejection, and feature selection.

## v26.43 — Deterministic Research Execution Skeleton
- Added validated market-event contract.
- Added research-only simulation intent contract with causal earliest-fill constraint.
- Added isolated ledger namespace and ledger-evidence contracts.
- Added multi-strategy research-batch identity validation with duplicate strategy rejection.
- Added regression coverage for OHLC integrity, causal fill timing, and multi-strategy isolation.
- No broker or live-account adapter was added.

## v26.44 — Deterministic Fill + Evidence Layer
- Added deterministic next-eligible-event fill model.
- Added explicit per-unit commission and slippage inputs.
- Added append-only SHA-256 evidence-chain contract.
- Added regression tests for deterministic fill and evidence-chain divergence.
- No live order route or broker integration was introduced.

## v26.45 — Native Build Stabilization + Simulated Account/Reporting Foundation
- Rebased the v26.45 working branch onto the confirmed v26.44 source baseline after detecting that main did not yet contain v26.41–v26.44.
- Corrected the native test-project compile failure by adding the explicit xUnit global using required by the test sources.
- Preserved the research-only authority boundary.
- Extended SimulationFill identity to retain buy/sell side so account-state semantics cannot infer direction from quantity.
- Added isolated simulated-account state with cash, long position, average entry price, realized P&L, unrealized P&L, and equity snapshots.
- Added reproducible research-report contract with explicit Complete, DataBlocked, and Invalid states.
- Data-blocked and invalid reports cannot carry simulated performance state.
- Added regression tests for account P&L, ledger isolation, and report blocking semantics.

### v26.45 validation
- Source-level review: completed.
- Native validation: submitted to GitHub Actions; final status was later corrected in v26.46.
- Local native compilation is not claimed.
- No trading performance numbers were generated.
- No live broker or live-account path was introduced.

## v26.46 — Native Stabilization Correction
- Corrected the demonstrated simulated-account regression expectation from realized P&L 8 to 9; production accounting code was unchanged.
- Confirmed the corrected branch with GitHub Actions run 36064442207: static repository gate passed, native restore/build passed, and tests passed.
- Documentation validation runs 36064646375 and 36064649780 also passed.
- No new authority, broker, live-account, or application-setting capability was introduced.
- Local native compilation is not claimed; GitHub Actions remains the authoritative native validation environment for this stage.

## v26.47 — Deterministic Research Runner + Simulation Ledger Integration
- Baseline reviewed from v26.46 commit f4f0df930e6730f3b6d60c7aa558e041ee899bbb before modification.
- Added ResearchRunRequest and DeterministicResearchRunner to connect admitted research jobs, simulation intents, ordered market events, deterministic fills, and isolated simulated accounts.
- The runner validates research authority, data admission, strategy admission, reproducibility identity, execution timing, intent identity, and market-event ordering before creating performance state.
- The runner selects the first market event at or after each intent's earliest eligible time; no same-bar shortcut or future-data access is introduced.
- Data coverage failures produce DataBlocked reports with no account/performance state.
- Admission failures produce Invalid reports with no account/performance state when the research identity is complete.
- Added deterministic repeatability, causal fill, blocked-data, invalid-admission, and multi-strategy isolation regression coverage.
- Added batch-level strategy/ledger independence enforcement.
- No live broker, live-account trading, automatic order submission, or application-setting mutation was introduced.

### v26.47 validation
- Source and repository integrity review: completed before modification; final branch comparison is 4 commits ahead of the v26.46 baseline with no unrelated file/folder changes.
- Native restore/build/test/static validation: passed in GitHub Actions run 36065367505 on final v26.47 commit 354d04f8486af1953e9f914e3bbe19fefe8f881c.
- Local native compilation is not claimed.
- This iteration does not add Android/Windows UI or platform publishing.
- No trading performance claim is made by the deterministic runner tests; their assertions validate engine mechanics only.

## Build governance / approval workflow
- Mellon initiates one controlled master-build inspection/planning cycle.
- The authoritative repository is Bakofritz/QuantForge-Trading.
- Each iteration starts by reviewing the last confirmed stable repository source and checking repository integrity.
- Planned build contents, affected paths, baseline SHA, branch, intended commit, and validation plan are presented before packaging or repository changes.
- The build remains queued until the user explicitly approves the exact proposal.
- Material changes to an approved proposal require a new approval.
- A successful scrub, simulation, replay, optimization, or research approval never grants LiveAccount authority.
- ZIP packaging is performed only after the approved repository state and commit are confirmed.
- Cumulative documentation is appended to running files rather than generating redundant documentation families.
- Failed iterations are cataloged separately from stable releases. A failed build remains historical evidence and is never labeled stable merely because a later correction exists.
- Correction iterations remain candidates until native build and tests pass; a correction does not inherit stable status from its predecessor.

## Progress estimate after v26.47
- Core research/simulation: 93%
- Safety/governance: 94%
- Strategy quarantine/scrubbing/import: 86%
- Historical-data integrity/provenance: 88%
- Read-only multi-strategy research: 89%
- Optimization/research admission: 84%
- Native Android/Windows production: 45–50%
- Full end-to-end implementation: 74–77%
- Overall usable research platform: 87–90%

These are engineering planning estimates, not runtime certification.

## v26.48 — Deterministic Execution Evidence + Multi-Intent Ledger Completion
- Baseline reviewed from v26.47 closeout commit 3d07acedf430937136d20868e6094651058eb97c before modification.
- Added deterministic research execution evidence rooted in the admitted research job identity.
- Every accepted simulation fill is appended to the existing SHA-256 evidence chain with canonical fill payload fields, preserving sequence and prior-fingerprint linkage.
- Complete research reports now require both a final simulated-account snapshot and an execution-evidence tail; DataBlocked and Invalid reports cannot carry either performance state or execution evidence.
- Multiple intents are processed in deterministic causal order and consume distinct ordered market events, preventing later intents from silently reusing an already-consumed event.
- Added regression coverage for multi-intent buy/sell lifecycle, insufficient-position rejection, commission/slippage accounting, evidence repeatability, and report evidence requirements.
- Existing admission, authority, causal-timing, strategy identity, and ledger namespace gates remain fail-closed.
- No live broker, live-account trading, automatic order submission, credentials/secrets, or application-setting mutation was introduced.
- No Android/Windows UI or platform publishing was added.

### v26.48 validation
- Repository integrity was reviewed from the confirmed v26.47 source tree before modification.
- GitHub Actions run 36067507767: static-contract-gate passed; restore passed; native build failed on test-project nullability error CS8629 in ResearchRunnerTests.cs line 144; tests were skipped.
- Status: FAILED / NON-STABLE.
- The complete failed-iteration record is cataloged separately under releases/failed/v26.48/ and releases/FAILED_ITERATIONS.md.
- Local native compilation is not claimed.
- No trading performance claim is made; regression assertions validate deterministic engine mechanics only.

## v26.49 — Native Build Correction + Failed/Stable Catalog Separation
- Baseline: v26.48 commit 815160dd454b0e29e337fce342a0e993cf3be259.
- Corrected the test-project nullable-value handling in ResearchRunnerTests.cs so the compiler can establish the account is present before accessing its value.
- Added explicit failed-iteration cataloging for v26.48, including its failing commit, CI run, compiler error, and non-stable classification.
- Added release-archive guidance distinguishing stable records, failed records, and correction candidates.
- Preserved all v26.48 execution-evidence behavior and research/live authority boundaries.
- No live broker, live account, automatic order submission, credentials, application-setting mutation, or Android/Windows UI was introduced.

### v26.49 validation
- Correction commit: 4a0257fc53f95ba3035b0d60d0a485659cac9a4e.
- Native GitHub Actions validation is required before v26.49 can be called stable.
- Local native compilation is not claimed.

## Failed-iteration automatic correction rule
- When a build iteration fails validation, the master-build process automatically enters a correction cycle rather than treating the failure as a stopping point.
- The correction cycle must test the failed revision directly in the authoritative build environment available to the project (currently GitHub Actions for native .NET validation), inspect the resulting compiler, test, static-gate, or other validation output, and identify the specific defect before applying a correction.
- Fixes must be evidence-driven and limited to the identified failure unless additional defects are directly exposed by the same validation cycle.
- After each correction, the affected build/test environment is rerun and the new results are analyzed before the iteration can advance.
- Failed iterations remain separately cataloged even when automatically corrected; a corrected iteration is a new correction candidate and does not inherit stable status.
- This automatic correction authority does not bypass the existing approval gate for repository-changing iterations: the exact proposed correction must be approved before its commit, unless the correction is already covered by an explicit prior approval for that exact iteration and scope.

## Mellon automatic correction and progression rule
- When the user says, “Mellon. Proceed with automatic error correction and stable master-build progression,” the master-build process interprets that instruction as authorization to continue the established correction-and-validation workflow within the currently approved iteration scope.
- The process automatically identifies documented build, compiler, test, static-gate, or validation failures; analyzes the actual authoritative results; applies evidence-driven corrections; and reruns the affected validation.
- A corrected iteration remains non-stable until the required authoritative build and tests pass.
- Once an iteration is confirmed stable, Mellon continues inspection and planning toward the next stable master-build iteration, while preserving the existing approval gate before any new or materially changed repository mutation.
- Mellon never bypasses required user approval, protected-main controls, safety boundaries, live-account restrictions, or the requirement for authoritative validation.
- Mellon progression is intended to continue as far as stability and validation permit rather than stopping merely because an individual error was encountered.

## Mellon multi-cycle correction and validation addendum
- Within a user-approved master-build iteration, Mellon may perform multiple sequential correction/validation cycles without requesting a new approval for each individual evidence-driven correction that remains within the exact approved iteration scope.
- Each cycle reviews the latest authoritative GitHub source and validation state, analyzes actual compiler/build/test/static-gate/data-integrity results, identifies the specific exposed defect, applies an evidence-driven correction, and reruns authoritative validation.
- The cycle may repeat as additional directly exposed defects are discovered. Mellon does not stop merely because the first correction reveals another failure.
- The iteration does not advance to stable status after compilation alone. After successful compilation, Mellon must run the required test suite and regression validation, then validate applicable data-integrity, reproducibility, authority-boundary, and safety contracts.
- Successful automated validation places the iteration in **QUEUED FOR USER REVIEW** status. Mellon then presents the complete build record, progress percentages, validation evidence, current hangups if any, and proposed stable-release status for manual approval under the existing approval rules.
- Mellon may not merge to protected main, promote the iteration to stable, package the final release, or make materially changed scope without the required human approval.
- If a required correction would materially expand the approved iteration scope, Mellon stops the autonomous cycle, queues the changed proposal, and requests a new approval.
- Failed iterations remain separately cataloged from corrected and stable iterations throughout the multi-cycle process.
- The multi-cycle rule does not grant live-account, live-broker, automatic-order, credential, or application-setting authority.

## Mellon domain-correctness flagging and non-blocking defect rule
- Mellon separates **program/build stability** from **domain-calculation correctness**.
- Compilation failures, broken basic program functions, safety/authority failures, data-integrity failures, broken research execution, evidence/provenance failures, and defects that could permit unauthorized live-account behavior remain blocking failures and must be corrected before stability can be declared.
- Domain-specific calculation defects may be logged and flagged without blocking overall program stabilization when the underlying program, build, core functions, safety boundaries, research execution, and required integrity contracts operate correctly.
- Examples include trade-calculation discrepancies, commission/slippage-model discrepancies, indicator calculations, market-analysis calculations, strategy-specific mathematics, optimization calculations, performance statistics, and calibration differences.
- A flagged domain defect must never be silently treated as correct. It must be recorded with the affected component, observed behavior, expected behavior when known, reproduction information when available, and an appropriate severity/priority for later correction.
- A build may therefore reach **STABLE PROGRAM BUILD / DOMAIN ISSUES FLAGGED** status when all blocking stability requirements pass while one or more non-blocking domain-correctness issues remain open.
- A stable program build does not mean that all trading or research mathematics are fully calibrated or correct.
- Mellon continues broader stabilization while carrying flagged domain defects forward for subsequent correction and validation cycles.
- The user must be clearly informed of material flagged domain defects when a build is presented for manual review.
- This rule does not weaken live-account restrictions, safety gates, data admission, authority separation, evidence requirements, or protected-main approval controls.

## Mellon prompt-form master rule — human-readable copy
> **MELLON:** Continue the QuantForge master-build process automatically within the approved scope. Inspect the latest authoritative source, find actual failures, correct them, rebuild, test, and validate. Repeat as many correction cycles as needed. Do not stop just because one correction reveals another problem. Treat **correction candidate** and **stable build** as complementary states: a correction candidate is a build being repaired and validated; a stable build is one that has passed all required blocking stability checks.
>
> Keep compilation, basic program operation, safety, authority boundaries, data integrity, research execution, evidence/provenance, and unauthorized-live-operation risks as **blocking requirements**. If one of these fails, keep correcting and validating.
>
> Calculation and domain-model problems such as trade math, commission/slippage calculations, indicators, market analysis, strategy mathematics, optimization calculations, performance statistics, and calibration can be **logged and flagged for later correction** when the underlying program remains stable and the defect does not violate a blocking safety, authority, integrity, or basic-function requirement. Never call a flagged calculation correct; record it clearly and carry it forward.
>
> After every correction cycle, use authoritative validation where available. Do not claim a build is stable without the required evidence. Once all blocking requirements pass, the build may be presented as a **stable program build with flagged domain issues**, if applicable, for manual review. Do not merge, promote, or release beyond the approval boundary without the required human approval.
>
> Never grant live-account, live-broker, automatic-order, credential, or application-setting authority through research, simulation, optimization, scrubbing, validation, or stabilization.


## v26.50 — Research Admission Failure Containment Hardening
- Baseline: v26.49 validated correction candidate at commit 137d772dd1ee6edfb00bec0a7130aa7ba2e6b764.
- Hardened the deterministic research runner so intent/strategy identity admission failures are converted into explicit **Invalid** research reports instead of escaping as unclassified runtime exceptions.
- Added regression coverage confirming that a strategy-identity mismatch produces no simulated account state and no execution evidence.
- Preserved fail-closed authority, data admission, causal execution, ledger isolation, and evidence-chain boundaries.
- No live broker, live account, automatic order submission, credentials, application-setting mutation, or platform UI capability was introduced.
- This correction addresses program-level failure containment; it does not certify domain-specific trading mathematics.

### v26.50 validation
- Source correction commits: 28a60e53d821a3b75f51ae8a54ea2dac97a2aff3 and 084e09345e675c783a25f6525d9085c405c9940e.
- Authoritative GitHub Actions validation is required before v26.50 can be classified as stable.
- Local native compilation is not claimed.


## Mellon Phase 1 full-automation addendum — user authorized
- The user authorizes Mellon to execute the full approved master-build automation process through **Phase 1**, including multiple sequential correction-candidate iterations, validation cycles, and stable-build transitions, without requesting separate approval for each individual iteration that remains within the approved Phase 1 scope.
- This authorization covers the entire Phase 1 production-progress objective and remains active until Phase 1 completion criteria are satisfied or Mellon identifies a materially expanded scope that cannot reasonably remain within Phase 1.
- Mellon must continue the correction cycle as many iterations as necessary: inspect the latest validated source, build, test, analyze actual failures, apply evidence-driven corrections, rerun authoritative validation, and repeat.
- Phase 1 core focus is **data integrity, core-function validity, and intra-program communication integrity**. These are priority blocking concerns during Phase 1.
- Phase 1 blocking validation includes compilation/build stability, basic core-function behavior, data admission and integrity, research execution integrity, authority separation, evidence/provenance integrity, and reliable communication of state/data between program components.
- Domain-specific calculation defects may remain explicitly flagged under the existing non-blocking domain-correctness rule when they do not compromise Phase 1 core integrity, safety, authority, or basic program operation.
- Mellon must not silently pass, suppress, or reinterpret data-integrity, core-function, or intra-program communication failures as non-blocking.
- Mellon must preserve the existing fail-closed safety model, live-account restrictions, protected-main controls, provenance/evidence requirements, and no-fabrication validation rules.
- Mellon may create and validate successive Phase 1 correction-candidate branches/PRs as required. Stable builds remain distinct from failed iterations and correction candidates.
- Mellon must alert the user **only when Phase 1 completion criteria have been met and Phase 1 is ready for manual transition approval to Phase 2**, rather than interrupting the user for each intermediate correction cycle.
- The user's Phase 2 approval will authorize the same full-automation model for Phase 2, continuing through all required correction and validation iterations until Phase 2 completion.
- The same progression model applies successively to later phases until the approved production roadmap is complete.
- This phase-level automation authorization does not authorize merging to protected main, final stable promotion, live-account operation, live credentials, automatic live orders, or any materially expanded scope without the applicable approval and safety gates.


## v27.00 — Phase 2 Integration Automation Kickoff
- Phase 1 completion was confirmed on v26.50 through authoritative GitHub Actions validation: static gate, restore, native compilation, and 29-test execution passed with 0 warnings/errors.
- User approval authorizes full Mellon automation through Phase 2 within the approved production roadmap, using the same inspect → build → test → analyze → correct → revalidate cycle.
- Phase 2 priority is integration hardening around the already-validated research core: data/strategy admission boundaries, quarantine/scrubbing interfaces, read-only multi-strategy research/optimization pathways, provenance/evidence propagation, and reliable component-to-component state communication.
- Phase 2 must preserve all Phase 1 blocking contracts: fail-closed data integrity, core-function validity, authority separation, evidence/provenance integrity, causal execution, and strict separation from live-account authority.
- Mellon may create successive correction-candidate branches/PRs and repeat validation without requesting approval for each ordinary Phase 2 correction cycle.
- Domain-calculation defects remain non-blocking only when they do not compromise Phase 2 core integrity, safety, authority, or basic program operation; they must remain explicitly recorded.
- No merge to protected main, live-account capability, live credentials, automatic live orders, or materially expanded scope is authorized by this phase approval.
- Phase 2 completion will be reported only after its blocking integration criteria are validated; then manual approval will be requested for Phase 3 transition.

### v27.00 initial Phase 2 validation state
- Branch: master/v27.00-phase2-integration.
- Baseline: v26.50 commit 2a4831c4ab20a1e4340f0597a71aa9f3ef529e03.
- Initial source inspection: completed.
- Authoritative Phase 2 validation after the kickoff documentation change: pending.
- This kickoff does not claim Phase 2 completion.


## QuantForge production phase plan — cumulative roadmap
This roadmap is appended to the master build record and is carried forward with every build. It defines the intended progression without treating future work as already implemented.

### Phase 1 — Core integrity foundation
**Status: COMPLETE**
- Primary objective: establish a trustworthy research core.
- Blocking focus: data integrity, core-function validity, intra-program communication, authority separation, evidence/provenance, and native build/test stability.
- Exit criteria: authoritative build/test validation passes; core research paths communicate valid state/data; blocked/invalid states remain fail-closed; live authority remains separate.
- Completed baseline: v26.50, authoritative GitHub Actions run 36076169410, 29 tests passed, 0 failed, 0 skipped, 0 warnings/errors.

### Phase 2 — Integration hardening
**Status: ACTIVE**
- Primary objective: connect the validated core components without weakening their contracts.
- Focus areas:
  1. Data admission → research execution communication.
  2. Strategy quarantine/scrubbing → feature selection → sanitized strategy admission.
  3. Multi-strategy identity and isolated ledger propagation.
  4. Read-only optimization/research across one or many scripts.
  5. Provenance and evidence propagation across component boundaries.
  6. Reliable component state/error communication and fail-closed handling.
- Blocking exit criteria:
  - no unauthorized authority crossing;
  - admitted data remains traceable and integrity-bound;
  - sanitized strategy identity remains linked to provenance;
  - multi-strategy jobs preserve isolation;
  - read-only research/optimization cannot submit orders or mutate application settings;
  - blocked/invalid conditions remain explicit and cannot become performance evidence;
  - authoritative native build/test/static/integration validation passes.
- Current branch: master/v27.00-phase2-integration.
- Current state: validation pending after Phase 2 kickoff documentation.

### Phase 3 — Research workflow completion
**Status: PLANNED — requires Phase 2 completion and user transition approval**
- Primary objective: complete the end-to-end user-directed research workflow from import through audited research results.
- Intended scope: workflow orchestration, research job lifecycle, batch/multi-script controls, optimization job management, reproducibility/reporting surfaces, and robust recovery of blocked/invalid jobs.
- Blocking focus: end-to-end data lineage, authority continuity, reproducibility, job isolation, and safe failure recovery.
- Exit criteria will be defined and recorded at Phase 3 kickoff from the validated Phase 2 baseline.

### Phase 4 — Product/UI integration
**Status: PLANNED — requires Phase 3 completion and user transition approval**
- Primary objective: connect the validated research services to production application interfaces.
- Intended scope: Android/Windows UI integration, user workflow state, import/audit controls, research dashboards, optimization controls, evidence/provenance presentation, and error/status communication.
- Blocking focus: UI must not bypass service-layer authority, safety, data, provenance, or live-account restrictions.
- Exit criteria will be defined at Phase 4 kickoff from the validated Phase 3 baseline.

### Phase 5 — Production hardening and release readiness
**Status: PLANNED — requires Phase 4 completion and user transition approval**
- Primary objective: harden the integrated product for controlled release.
- Intended scope: packaging, deployment validation, performance/resource testing, recovery behavior, compatibility, security review, documentation closeout, and release evidence.
- Blocking focus: reproducible builds, security/safety boundaries, data integrity, failure recovery, and release traceability.
- Exit criteria will be defined at Phase 5 kickoff from the validated Phase 4 baseline.

### Phase 6 — Live-account readiness review
**Status: PLANNED / SEPARATELY GOVERNED**
- Primary objective: evaluate whether a separately authorized live-account capability should be designed and implemented.
- Live-account authority is never inherited from research, simulation, optimization, scrubbing, replay, UI integration, or release completion.
- Any live-account implementation requires a distinct scope proposal, explicit safety/authority design, dedicated validation, and the applicable human approvals before credentials, broker connectivity, or order submission are introduced.
- Phase 6 is therefore not an automatic grant of live trading capability.

### Phase progression rule
- Mellon works continuously within the currently approved phase.
- Ordinary correction-candidate iterations inside that phase do not require separate approval when already covered by the phase authorization.
- Mellon must stop and request approval only for a material scope expansion or when the phase's blocking completion criteria have been met and transition to the next phase is ready for human approval.
- Every build record must state the current phase, phase objective, phase completion criteria, current phase status, and the next phase.
- Future phases are planning targets, not claims of implemented functionality.


## Per-build phase-plan recording rule
Starting with v27.00 and every subsequent build, the build record must append a concise phase-plan entry containing:
- build/iteration identifier;
- active phase;
- phase objective;
- work completed in that build;
- blocking criteria being validated;
- non-blocking domain issues carried forward;
- validation state;
- phase completion percentage;
- overall QuantForge progress estimate;
- next planned phase action.
This is cumulative: update the running documents rather than creating a new phase-plan document for every iteration.


## Mellon complete-build ZIP archive export rule — user authorized
- In addition to the GitHub master-build process, every future QuantForge build iteration that is presented as a build artifact must also be exported as a **complete ZIP archive** in the chat when the exact build contents are available.
- The ZIP is a companion archive, not a replacement for GitHub. GitHub remains the authoritative source for repository history, commits, branches, PRs, and validation.
- The ZIP must contain the **entire build state being delivered**, including source, tests, documentation, phase/build records, user manual, configuration/build files, scripts/tools, release evidence applicable to that build, and other repository files required to reproduce or inspect the delivered build, subject to exclusion of transient/generated secrets, credentials, caches, and other unsafe or non-source material.
- The archive must preserve the repository's relative directory structure.
- Do not create a ZIP and call it complete unless the exact archive path and contents have been verified.
- The build report must identify the ZIP filename/path, archive scope, corresponding GitHub commit SHA, branch, and validation state.
- If an archive cannot be generated or its completeness cannot be verified in the current environment, Mellon must say so explicitly rather than claiming that the ZIP exists.
- Failed and correction-candidate builds may also be archived when useful, but their filename and build record must clearly identify their non-stable status; a failed build must never be presented as a stable release.
- Stable-build ZIP archives should be retained as distinct release evidence and should correspond to the exact validated commit being presented.
- The ZIP export does not authorize a merge to protected main, stable promotion, live-account capability, or any other approval-gated action.
- This rule applies to future builds from the current Mellon state forward and should be preserved in subsequent build-document updates.

## v27.01 — Phase 2 Integration Correction Candidate

- Active phase: **Phase 2 — Integration hardening**.
- Branch: `master/v27.00-phase2-integration`.
- Baseline: validated v26.50 Phase 1 foundation; Phase 2 remains unmerged and independently validated.
- Work completed: hardened the deterministic research runner so malformed market-event input and simulation execution failures are converted into explicit `Invalid` research reports instead of escaping as uncontained runtime failures; enforced that the simulated ledger namespace strategy identity matches the admitted strategy; added regression coverage for ledger identity mismatch and execution-domain failure containment; added CI packaging of the complete source tree as a build artifact for the required chat ZIP export workflow.
- Blocking criteria addressed: research execution containment, multi-strategy/ledger identity isolation, explicit invalid-result semantics, reproducible build packaging.
- Non-blocking domain issues: no new domain-calculation defect was established by this iteration.
- Validation: authoritative GitHub Actions run `36078006498` for the prior source correction was still in progress at the time of this record; the newest workflow commit will receive its own authoritative run. Therefore this iteration is **CORRECTION CANDIDATE / VALIDATION PENDING**, not stable.
- Phase 2 progress estimate: **approximately 25%** (engineering estimate; integration scope remains substantial).
- Overall QuantForge progress estimate: **approximately 79%**.
- Next action: inspect the newest authoritative CI result, correct any exposed failures, then package and verify the exact validated commit archive. No merge to `main` or live authority is implied.



## v27.11 — Phase 2 Exit-Contract Validation

- Active phase: **Phase 2 — Integration hardening**.
- Validated head: `bb25c93ded5aa53d38cca8d7c6b99434720fec4a`.
- Authoritative CI: GitHub Actions run `36083935121` — **SUCCESS**. Static contract gate, restore, Release build, tests, complete-source packaging, and artifact upload all passed.
- Stable artifact: `QuantForge-complete-build-15048e7edfe2fb7a18d8faca58c69459dcc98d3d`, artifact ID `10843232577`, SHA-256 `1b4a7fb441e50d2243570a4457ac869d80a1ec38e228bbed14bf11da25ba7681`.
- Work completed across Phase 2: fail-closed strategy admission; sanitized fingerprint binding; admission-to-execution binding; read-only multi-strategy orchestration; provenance/evidence binding; explicit component terminal/error state; per-strategy batch failure containment; consolidated exit-contract validation.
- Blocking criteria validated: no authority crossing in the tested research path; admitted data/job identity traceability; sanitized strategy provenance; independent strategy execution; no order/settings/live-account authority from read-only orchestration; invalid/blocked results remain non-performance evidence; explicit terminal state/error propagation.
- Failed correction history retained: v27.08 initial provenance-test fixture failure and v27.10 initial canonical-identity compile failure remain non-stable evidence; both were corrected and independently revalidated.
- Phase 2 engineering implementation and blocking exit-contract validation: **100%** for the approved Phase 2 scope.
- Overall usable research platform estimate: **99%**. Full end-to-end QuantForge implementation estimate: **90%**; later phases remain intentionally incomplete.
- Live environment: disabled/separately governed. No broker credentials, live orders, or live-account authority were introduced.
- Next action: documentation/release-evidence synchronization is this closeout iteration; after its own CI passes, stop before Phase 3 and request human transition approval.

## v28.06 — Phase 3 Research Workflow Closeout

- Active phase: **Phase 3 — Research workflow completion**.
- Validated Phase 2/main baseline: merge commit `a0ff34925d2ee48a8de83c22b53519e43f167e63`.
- Phase 3 branch: `master/v28.00-phase3-research-workflow`.
- Implemented across v28.00-v28.05: reliability-gated research admission; explicit unresolved-gap/conflicting-overlap blocking; isolated optimization variant planning; causal multi-timeframe closed-bar/information-availability validation; deterministic workflow summaries and Markdown export; publication binding of complete results to strategy provenance, execution evidence, and matching data reliability; consolidated Phase 3 exit-contract coverage.
- Blocking criteria validated: research data lineage remains fingerprint-bound; missing live-benchmark comparison and unresolved data defects cannot silently become performance evidence; optimization variants retain unique reproducibility identity and ledger namespaces; higher-timeframe information must be closed and available at decision time; unsafe order/live/settings authority remains fail-closed; blocked/invalid jobs contain no simulated account state or execution evidence; publishable results require complete provenance and reliability identity.
- Authoritative Phase 3 CI checkpoints: v28.00 run `36085788992` SUCCESS; v28.01 run `36085885453` SUCCESS; v28.02 run `36086003716` SUCCESS; v28.03 run `36086120712` SUCCESS; v28.04 run `36086183938` SUCCESS; v28.05 exit-contract run `36086318725` SUCCESS.
- Phase 3 engineering and blocking exit-contract implementation: **100%** for the approved research-workflow scope. This is not a claim that Product/UI, production-release hardening, or separately governed live-account capability is complete.
- Overall usable research platform estimate: **99%**. Full end-to-end QuantForge implementation estimate: **94%**; Phase 4 Product/UI integration and later hardening remain.
- Non-blocking domain issues carried forward: no new calculation defect was established by the Phase 3 blocking-contract suite; real strategy performance, commissions/slippage calibration, indicator mathematics, and market-model calibration remain subject to later domain validation with representative data.
- Live environment: **disabled and separately governed**. Phase 3 introduced no broker credentials, live order route, live-account authorization, or application-setting mutation authority.
- Superseding roadmap status: Phase 1 COMPLETE; Phase 2 COMPLETE and merged to `main`; Phase 3 COMPLETE at engineering/exit-contract level pending final branch closeout/merge approval; Phase 4 remains PLANNED and requires transition approval.
- Next action: remove the one-time documentation-sync workflow, run the final authoritative quality gate, package the exact closeout artifact, open the Phase 3 pull request, and stop before merge/Phase 4 transition for the applicable human approval.

## v29.05 — Phase 4 Product/UI Integration Progress Checkpoint

- Active phase: **Phase 4 — Product/UI integration**.
- Validated Phase 3/main baseline: squash-merge commit `5c664b45d7fe7bdbc3cea1c617c2242c7439c307`; post-merge Quality Gate `36087253786` passed.
- Phase 4 branch: `master/v29.00-phase4-product-ui`.
- v29.00 added a read-only Product UI boundary that converts validated research workflow summaries into immutable UI state while hard-coding live-account, order-submission, and application-setting authority to false. Quality Gate `36087453493` passed.
- v29.01 added strategy audit/quarantine presentation. Unsafe capabilities remain visible to the user but cannot be represented as research-admitted, and admitted strategies are revalidated through the existing admission pipeline. Quality Gate `36087589826` passed.
- v29.02 added read-only optimization-plan presentation preserving unique job fingerprints, parameter identities, temporal partitions, and isolated ledger namespaces. Unsafe live-authority variants fail closed. Quality Gate `36087752854` passed.
- v29.03 added explicit data-reliability presentation. Missing live benchmark comparison, unresolved gaps, and conflicting overlaps remain visibly blocked rather than being inferred or hidden. Quality Gate `36087833540` passed.
- v29.04 added a fail-closed UI research-job lifecycle for Pending, Running, Complete, DataBlocked, and Invalid states. Only blocked/invalid jobs can be explicitly reset for retry; completed jobs cannot silently rerun. Quality Gate `36087927349` passed.
- Blocking contracts preserved: UI state cannot create broker/live authority, submit orders, mutate application settings, self-promote quarantined strategies, hide data-integrity limitations, merge isolated optimization ledgers, or reinterpret blocked/invalid research as completed performance evidence.
- Phase 4 engineering progress estimate: **approximately 40%**. Remaining blocking Phase 4 work includes actual Windows/Android application shells, service-to-view-model wiring, platform build validation, end-to-end UI boundary tests, and Phase 4 exit-contract validation.
- Overall usable research platform remains approximately **99%**. Full end-to-end QuantForge implementation estimate: **approximately 95%**. These are engineering planning estimates, not production certification.
- Live environment: **disabled and separately governed**. No broker credentials, live-order route, automatic live-order authority, or settings-mutation authority was introduced.
- Next action: continue Phase 4 with platform-neutral application state/view-model contracts, then establish authoritative Windows/Android shell/build validation without weakening the service-layer safety boundaries.

## v29.10 — Phase 4 Product/UI Integration Closeout

- Active phase: **Phase 4 — Product/UI integration**.
- Validated Phase 3/main baseline: squash-merge commit `5c664b45d7fe7bdbc3cea1c617c2242c7439c307`; post-merge Quality Gate `36087253786` passed.
- Phase 4 branch: `master/v29.00-phase4-product-ui`.
- v29.00-v29.05 established the read-only Product UI boundary, strategy audit/quarantine presentation, isolated optimization presentation, explicit data-reliability presentation, fail-closed job lifecycle, and cumulative Phase 4 documentation checkpoint.
- v29.06 added a platform-neutral application view-model contract. It rejects forged live/order/settings authority, requires complete workflow identity and reliability state, requires execution evidence for completed jobs, preserves explicit blocked/invalid reasons, and exposes only section-appropriate research operations.
- v29.07 added the product application coordinator that wires validated research summaries through the read-only UI boundary into application state and validates UI commands. Initial commit `12e6d8600a40c62967933243e17191a543cbd8c9` failed because tests referenced a nonexistent authority enum member; correction v29.07b `bc9a9b0ece10c61fbdea5e3c6aa8417efadabc58` used `ReadOnlyResearch` and passed authoritative validation.
- v29.08 introduced actual .NET MAUI Windows and Android research application shells. Initial commit `4ad9d828d6c833c339f78cb38397fc5b57c7b75d` exposed cross-platform restore failure `NETSDK1100`; v29.08b `6d4effe1fd3a983fbbe97a2247a5186b612f774a` added explicit Windows targeting support and the MAUI Controls package reference. Quality Gate `36089225994` then passed static, core test, Android shell build, and Windows shell build jobs.
- v29.09 bound the MAUI shell to validated `ProductApplicationViewModel` state so workflow identity, workspace section, reliability status, research-command availability, job terminal status, blocked/invalid reasons, and execution evidence can be displayed without creating authority. Consolidated Phase 4 exit-contract tests were added. Initial commit `f7d864129c5ca870e5d59396de1889192c972e65` failed xUnit analyzer rule `xUnit2031`; v29.09b `5613fb8042f60ad535307dc563215d49a0bd4dca` corrected the assertions.
- Authoritative Phase 4 exit validation: Quality Gate `36090046532` passed all four jobs: static contract gate, core restore/build/test/package, Android MAUI shell build, and Windows MAUI shell build. Core build reported **0 warnings and 0 errors**; tests reported **95 passed, 0 failed, 0 skipped**.
- Blocking Phase 4 contracts validated: the UI cannot manufacture research admission or execution evidence; data reliability blocks remain visible and disable research execution; blocked/invalid jobs remain non-performance states; workspace operations remain constrained; live-account commands fail closed; Windows and Android shells consume the same read-only application contracts; no broker/order/settings authority was added.
- Phase 4 engineering and exit-contract implementation: **100% for the approved Phase 4 scope**. This does not certify final visual polish, store/distribution publishing, production hardening, trading-domain calibration, or separately governed live-account capability.
- Overall usable research platform estimate: **99%**. Full end-to-end QuantForge implementation estimate: **approximately 97%**. These are engineering planning estimates, not production certification.
- Live environment: **disabled and separately governed**. No broker credentials, live-order route, automatic order authority, or application-setting mutation authority was introduced.
- Next action: validate this documentation-closeout tree with the complete Quality Gate, package the exact validated archive, open the Phase 4 pull request against protected `main`, and stop before merge/Phase 5 transition for human approval.
