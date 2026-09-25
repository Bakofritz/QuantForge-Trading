# QuantForge User Manual

## What QuantForge is

QuantForge is a research and controlled-simulation platform for strategy auditing, market-data validation, causal backtesting/replay, read-only optimization, and simulated forward testing.

Research approval never grants live-account authority.

## The six safety gates

1. Data gate — market data must be identified, fingerprinted, structurally validated, and admitted.
2. Strategy gate — imported code is quarantined and its capabilities are inventoried.
3. Feature-selection gate — the user may select research-safe strategy components; order submission and application-setting mutation are excluded from research authority.
4. Authority gate — every job receives an explicit authority domain.
5. Causal/execution gate — no future information and no silent same-bar fills.
6. Evidence gate — results are bound to reproducibility identities and provenance.

A failed gate blocks the job.

## Strategy import workflow

quarantine → original fingerprint → static scrub → capability inventory → user feature selection → sanitized artifact → regression validation → provenance registration → research admission.

The original source remains distinct from the sanitized representation.

## Research domains

- Historical research
- Simulated replay
- Simulated account
- Read-only research/optimization
- Live account — separately restricted

Research jobs cannot submit orders or change application settings.

## Optimization

Optimization is read-only. It may evaluate one or many admitted strategies against approved market data.

Every run must identify its dataset, strategy, execution policy, parameter set, temporal partition, and job identity.

If authoritative data is unavailable or incomplete for the requested claim, QuantForge must report a data-blocked state instead of manufacturing performance.

## Simulated account

A simulated account is isolated by ledger namespace. A fill from another namespace is rejected.

The current account contract tracks:
- cash
- long position quantity
- average entry price
- realized P&L
- unrealized P&L at a supplied market price
- equity

A sell cannot exceed the simulated long position. Short-position accounting is not implicitly enabled by this contract.

## Deterministic research runner

The research runner is the controlled path that connects an admitted research job to simulation.

Workflow:
1. Validate authority, data, strategy, job identity, and execution timing.
2. Validate that every intent belongs to the admitted strategy and one isolated ledger namespace.
3. Validate and order the supplied market-event sequence.
4. For each intent, locate the first market event whose timestamp is at or after the intent's earliest eligible fill time.
5. Apply the deterministic fill model with explicit commission and slippage.
6. Apply fills to the isolated simulated account.
7. Value the final account against the final admitted market close.
8. Emit a reproducibility-bound research report.

If any requested intent has no eligible market event, the runner emits DataBlocked and does not create performance state.

If admission validation fails after a complete research identity is present, the runner emits Invalid and does not create performance state.

## Multi-strategy research

Batch execution requires unique strategy identities and independent ledger namespaces.

One strategy cannot consume another strategy's simulated ledger. A shared strategy identity is rejected before batch execution.

## Research reports

A report is reproducible only when its job, dataset, strategy, execution policy, parameter set, and temporal partition identities are present.

Report states:
- Complete — includes a validated simulated account snapshot.
- DataBlocked — explains why authoritative data did not support the requested research claim and contains no performance state.
- Invalid — indicates an invalid research result and contains no performance state.

## Causal integrity

At observation time T, only information available at or before T may be used. Unfinished higher-timeframe bars are not treated as observed information.

Violations are hard rejection conditions.

## Execution timing

Default research execution uses next-bar-open or next-eligible-tick semantics. Same-bar execution requires an explicit close-auction model.

The deterministic runner never converts a signal into a same-bar fill merely because an event is present in the same dataset.

## Provenance

An imported artifact should retain:
- source URI
- retrieval time
- SHA-256
- original fingerprint
- sanitized fingerprint
- scrub-report identity

## Build-status catalog

QuantForge keeps failed iterations separate from stable releases and from correction candidates.

- **Stable:** only an iteration whose required native build and tests have passed.
- **Failed:** an iteration with a documented validation failure. It remains historical evidence and is not promoted by later fixes.
- **Correction candidate:** an iteration intended to repair a failed build. It remains non-stable until independently validated.

For example, v26.48 is cataloged as a failed iteration because its GitHub Actions native build stopped on compiler error CS8629. Its source and CI evidence remain available for historical reference, while v26.49 is treated as a separate correction candidate.

## Current implementation status

The native .NET source and architecture tests are built incrementally. GitHub Actions is the authoritative native build/test environment for this stage.

A green test workflow is evidence for the tested source revision only; it is not a claim of completed Android/Windows product functionality.

## Troubleshooting

Import blocked: inspect the capability inventory and feature-selection result.

Research job blocked: inspect the first failed admission gate.

Backtest blocked: verify data admission, temporal partition, and execution timing.

Optimization has no performance: verify that authoritative data bytes were actually admitted. A data-blocked result is expected when source coverage is incomplete.

Runner returns DataBlocked: verify that the admitted market-event sequence reaches every intent's earliest eligible fill time.

Runner returns Invalid: inspect the report block reason and correct the failed admission contract; do not bypass the admission gate.

CI not green: do not treat the presence of tests as proof of a passing build; use the workflow result.

## v26.48 additions

The deterministic runner now produces an append-only execution-evidence chain for completed simulations. The evidence root is derived from the admitted research job identity, and each accepted fill is linked by sequence number and prior fingerprint.

Multiple simulation intents are processed in chronological signal order and consume successive eligible market events. This means a later intent cannot silently reuse the market event already used by an earlier intent.

A Complete report must contain both simulated account state and an evidence-chain tail. DataBlocked and Invalid reports contain neither. This keeps blocked or invalid research from becoming performance evidence.

The simulated account lifecycle is covered by regression tests for buy/sell, insufficient position rejection, commission, slippage, final valuation, and repeated deterministic evidence.

## v26.49 additions

v26.49 corrects the native test-project nullability compile failure identified by GitHub Actions for v26.48.

The correction does not weaken research admission, execution timing, evidence-chain, simulated-account, or live-account authority boundaries. It is independently validated and classified as stable only if the complete required GitHub Actions workflow passes.


## v26.50 additions
v26.50 hardens research admission failure handling. When a simulation intent does not belong to the admitted strategy, the deterministic research runner now returns an **Invalid** report rather than allowing that admission failure to escape as an unclassified runtime exception.

The invalid report contains no simulated account state and no execution-evidence tail. This keeps malformed research requests fail-closed while preserving the distinction between invalid admission and data-blocked execution.

This is a program-stability correction. It does not mean all trading calculations, indicators, market analysis, optimization mathematics, or calibration have been certified correct.


## v27.00 Phase 2 additions

Phase 2 continues from the validated research core and focuses on integration hardening. Mellon will validate the interfaces between data admission, strategy quarantine/scrubbing, research admission, read-only multi-strategy optimization, provenance/evidence, and downstream research execution.

Phase 2 remains research-only. Strategy scrubbing, optimization, replay, simulation, and validation do not grant live-account authority, order submission authority, credentials, or application-setting mutation authority.

A Phase 2 build is not considered complete until its required integration and blocking safety/data-integrity tests pass in authoritative validation.


## Cumulative production phase plan
The build documentation carries the authoritative engineering phase plan. In plain language, the roadmap is:

1. **Phase 1 — Core integrity foundation: COMPLETE.** Make sure the research engine, data flow, safety boundaries, and internal communication are trustworthy.
2. **Phase 2 — Integration hardening: ACTIVE.** Make the existing parts work together reliably: data admission, strategy quarantine/scrubbing, feature selection, multi-strategy research, read-only optimization, provenance/evidence, and component state/error communication.
3. **Phase 3 — Research workflow completion: PLANNED.** Finish the complete user-directed research workflow from import/audit through research and reproducible results.
4. **Phase 4 — Product/UI integration: PLANNED.** Connect validated research services to Android/Windows user interfaces without allowing the UI to bypass safety or authority controls.
5. **Phase 5 — Production hardening and release readiness: PLANNED.** Harden packaging, deployment, performance, recovery, compatibility, security, documentation, and release traceability.
6. **Phase 6 — Live-account readiness review: SEPARATELY GOVERNED.** Research completion does not activate live trading. Any live-account capability requires a separate scope, safety design, validation, and explicit approvals.

Every build should state which phase it is in, what that phase is trying to accomplish, what was completed, what remains, what is being tested, and the estimated progress toward the phase and overall product goal. Future phases are plans, not claims that those capabilities already exist.

## v27.01 Phase 2 Build Note

This iteration strengthens the research safety path. If a research run receives malformed market data or encounters a simulation execution problem, the runner now reports the run as **Invalid** instead of allowing the error to escape without a research result. The simulated ledger identity is also checked against the admitted strategy identity so one strategy cannot accidentally execute through another strategy's ledger namespace.

The build process now packages the complete source tree through the quality gate so a verified archive can be attached to the Mellon chat report and matched to the exact GitHub commit. This archive is a companion to GitHub; GitHub remains the authoritative source.

Phase 2 remains active. Live-account authority is still separate and is not granted by this work.



## v27.11 Phase 2 closeout note

Phase 2's integrated research path has passed its consolidated exit-contract validation. In practical terms, QuantForge now has a tested research-only chain connecting admitted data and sanitized strategies to isolated read-only research execution, contained failures, reproducible evidence/provenance, and explicit completion/error states.

The Phase 2 validation does **not** activate live trading. Live broker connections, credentials, real order submission, and research-driven application-setting changes remain outside this authority boundary.

The validated v27.11 source head is `bb25c93ded5aa53d38cca8d7c6b99434720fec4a` with successful GitHub Actions run `36083935121`. Phase 3 begins only after the Phase 2 closeout documentation/release-evidence revision passes its own validation and the user approves the phase transition.

## v28.06 — Phase 3 research workflow behavior

QuantForge now has a validated core workflow for coordinating research and optimization runs before Product/UI integration.

- **Data reliability is part of admission.** A dataset carries a reliability score and records whether it was compared with a live benchmark. If the dataset has unresolved gaps, conflicting overlapping records, or lacks the required benchmark comparison, the research job is blocked rather than guessing what the missing data should have been.
- **Optimization variants remain isolated.** Parameter variants must have unique research identities and separate ledger namespaces so one variant cannot reuse another variant's simulated account state.
- **Multi-timeframe research is causal.** A higher-timeframe bar must be closed, and its information must actually have been available at the decision time. An unfinished future bar cannot be used to improve an earlier decision.
- **Results are reproducible and shareable.** Completed workflow results can be summarized with a deterministic workflow fingerprint and exported as a readable Markdown report containing run status, reproducibility identity, evidence identity, and data-reliability information.
- **Publication is stricter than execution.** A result can be treated as publishable research evidence only when it is complete and can be bound to the admitted strategy provenance, execution evidence, and the matching reliability-assessed dataset.
- **Blocked and invalid runs do not become performance claims.** They contain an explicit reason and do not carry simulated account performance or execution evidence as though the run had completed normally.
- **Live trading remains disabled.** These research and optimization capabilities do not submit live orders, change application settings, unlock broker credentials, or grant live-account authority.

Phase 3 validates the research-service workflow. Windows/Android Product/UI integration is a later phase and should not be interpreted as complete merely because the Phase 3 core contracts pass.

## v29.05 — Phase 4 Product/UI integration behavior

The Phase 4 interface layer now has tested rules for showing research information without gaining trading authority.

- Research results shown to the interface keep their real Complete, Data Blocked, or Invalid status. A blocked or invalid run is not displayed as successful performance.
- Strategy audit screens can show unsafe capabilities such as order submission, settings changes, live-account requirements, or process/native execution. Seeing those capabilities does not approve them. Unsafe or incomplete strategies remain outside research admission until the existing safety pipeline validates them.
- Optimization variants keep separate research identities and simulated ledgers so one variant cannot silently reuse another variant's simulated account state.
- Data reliability is visible. Missing live-benchmark comparison, unresolved gaps, or conflicting overlaps remain blocking conditions and their limitations stay available to the interface.
- Research jobs move through explicit Pending, Running, Complete, Data Blocked, or Invalid states. Blocked/invalid jobs can be deliberately reset for another research attempt; completed jobs do not silently rerun.
- Live trading remains disabled. The interface contracts do not submit broker orders, unlock broker credentials, change application settings, or grant live-account authority.

These contracts are the safety-facing foundation for the later Windows and Android application screens. The actual platform application shells and platform build validation remain Phase 4 work in progress.

## v29.10 — Windows and Android research application shell

QuantForge now includes build-validated .NET MAUI research shells for Windows and Android. Both platforms use the same read-only Product/UI safety contracts rather than implementing separate trading authority.

The application shell can display the research workflow identity, active workspace, data-reliability condition, whether research commands are currently available, and each research job's status. Completed jobs must carry execution evidence. Data Blocked and Invalid jobs keep their real reasons visible and are not displayed as successful performance.

When the reliability layer reports an unresolved gap, conflicting overlap, missing benchmark comparison, or another blocking reliability condition, research execution remains disabled while the limitation can still be viewed. The interface does not fill missing market data by assumption.

Live trading remains disabled. The Windows and Android shells do not hold broker credentials, do not submit broker orders, do not grant live-account authority, and do not change protected application settings. Completing a research run, optimization, strategy audit, or platform build does not unlock live trading.

Phase 4 establishes the functional application-shell and safety foundation. Final visual design refinement, distribution/store packaging, broader production hardening, and separately governed live-account readiness remain later work.


## v30.00 — Reliability and release hardening

If reliability information is absent, refers to another dataset, or contains duplicate assessments, the research interface keeps execution disabled. Reports remain readable. Ready-state scores come from the matching assessments; missing coverage must not look like a verified score.

Source archives now include SOURCE_MANIFEST.json with the Git commit, source tree, and every source file's SHA-256. A companion .sha256 file checks the archive itself. These identify source bytes; they do not certify device behavior or trading profitability.

The application is primarily C#/.NET 10 with Windows/Android .NET MAUI shells. The sunflower reference is the intended final interface. The present shell is basic; full charting, script translation, AI/scout/bot operation, and complete import-to-results device workflows are not all implemented. Live-account authority remains disabled.


## v30.01 — Startup and rejected-refresh behavior

At startup the shell shows that validated research data is not loaded, reliability is not admitted, and research commands are disabled. Its diagnostic is QF-AWAITING-DATA.

When a summary is supplied on the UI thread, old displayed jobs are cleared before validation. Invalid input shows QF-PRESENTATION-REJECTED and leaves research commands disabled. Correct the input and submit a fresh valid summary; old results are never silently reused. Expected validation errors are contained; unexpected runtime exceptions propagate after clearing the session, with the shell still showing a rejection diagnostic. Raw internal exception details are not displayed.

QF-DATA-BLOCKED retains read-only visibility while blocking research execution. QF-PRESENTATION-READY means the presentation input passed validation; individual reports may still be Invalid or DataBlocked. It does not mean profitability, completed research, calibrated trade mathematics or live authority.

Undefined authority values, unknown research modes and duplicate job identities are rejected. The session is a display lifecycle, not a substitute for the core data/strategy/evidence admission gates. Native build validation does not replace real Windows/Android runtime testing.


## v30.02 — Inspect a research manifest

Select **Inspect research manifest**, then choose a QuantForge version-1 JSON manifest. The app reads only your selected file through the native provider stream. It displays the dataset and strategy identifiers and a SHA-256 fingerprint of the exact file bytes when inspection succeeds. This is metadata inspection, not a market-data import, strategy approval, or backtest. Research commands are not enabled by it.

The manifest must contain every canonical property exactly once, including an explicit supported research authority. Missing/duplicate/unknown properties, unsupported versions, live or undefined authority, invalid UTF-8, incomplete/overlong identities and files over 64 KiB are rejected. A UTF-8 BOM is accepted and remains part of the source hash.

Cancelled, unreadable and invalid inputs have distinct states; raw provider error details are not displayed. A failed attempt clears old inspection text and permits selecting another file. The selected source file is not modified, no stored setting is changed, and no path or script referenced inside the manifest is opened or executed. The native Android picker can create a temporary cached copy before returning the selection; the reader's 64 KiB budget does not bound that earlier platform copy. Reads use cooperative cancellation; a provider that ignores cancellation may still delay completion. Real device picker behavior remains to be tested beyond platform compilation.


## v30.03 — Inspect an NT8 one-minute export

Declare the exact contract (for example MES 09-26) and choose Last, Bid or Ask, then choose Inspect NT8 one-minute export (UTC). This reader accepts NT8 semicolon-separated UTF-8 text in yyyyMMdd HHmmss;open;high;low;close;volume format, with UTC end-of-bar timestamps aligned to the minute. Maximum 8 MiB, 100,000 bars and 256 characters per row. Day/tick exports and third-party local-time text are unsupported here. Keep MES/MNQ and Last/Bid/Ask files separate. The file's rows cannot verify your declared contract or series.

The report shows parsed bars, UTC range, original-byte SHA-256 and intervals that are not adjacent minutes. A session closure is not automatically a data gap; no bars are invented or removed. Duplicate/out-of-order timestamps and malformed rows reject the entire file. Cancellation/provider errors leave no partial inspected dataset. Correct the input and retry. Inspection does not admit data or strategies and does not enable backtesting. A trusted identity, session/coverage assessment and independent benchmark are still needed.

The core reader's limit does not bound MAUI's native provider caching before OpenReadAsync. Parsing runs off the UI thread; provider handling and the 30-second read token remain cooperative. Native startup, provider caching and cancellation still require runtime validation. This build is not a usable Android release.

APK delivery rule: only provide the user an APK once the complete agreed usable workflow has passed Android runtime validation, including import, supported strategy simulation, results/chart/ledger, recovery and documented safety limits. Compilation alone is insufficient. Source/review ZIP packages continue; no unfinished APK is included. Live-account authority remains disabled.


## v30.04 — Cancel and retry inspection

Cancel inspection requests cooperative cancellation for either file inspection. Leaving the page also requests cancellation. If a native picker is open, close or cancel that picker to return to the app. The app waits for the provider to return before enabling a new inspection; it never interprets cancellation as successful inspection or admission. Platform picker overlays, activity recreation, process termination and unresponsive providers still require native runtime testing. No hard deadline for external providers is promised.


## v30.05 — Compare declared sources and clear data

After a successful minute inspection, select Compare export with same declared contract and series. The chosen reference must be another UTC one-minute export for that declared contract and Last/Bid/Ask series. The app cannot prove this from the text rows; do not select an unrelated instrument or price series. It reports matching bars, differing OHLCV bars and timestamps found in only one source across their full observed ranges. No session calendar or missing bars are inferred. Identical bytes are flagged; different bytes do not prove independent provenance.

Agreement is not a live-data reliability score and cannot enable a research job. Independent reference provenance, real instrument mapping and session coverage remain required. Clear inspected market data releases the app's retained in-memory primary bars and comparison text. Changing a declaration or starting a new primary inspection also clears them. This does not delete your original files or promise cleanup of native picker cache files. Inspection results are not persisted across process restart. No APK or full working backtester is released by this feature.
