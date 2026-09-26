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


## v30.06 — S25 Ultra diagnostic APK test

This is QuantForge Test, an early diagnostic build, not a completed backtester. Install the APK on your phone, open the app, and tap Start test session. This replaces any previous session, so export a previous session first. Try the available manifest/minute inspection, compare, cancel and clear controls. Try backgrounding and returning to the app. Mark problem records a marker and your optional note; notes are included in the exported ZIP, so do not enter secrets.

Tap Stop session, then Export diagnostics ZIP. Choose a destination in Android's share sheet and upload QuantForge-diagnostics.zip in this chat. Export also stops a running session. Diagnostics remain local unless you explicitly share. Clear diagnostics removes the app's journal and cached export; it cannot remove copies already shared elsewhere. After an unexpected exit, reopen the app and export the existing session BEFORE starting another. A missing stop marker is labelled interrupted/unknown, not asserted to be a crash.

Recorded: fixed app action names/outcomes, UTC and elapsed times, operation durations, approximate managed-memory samples, model/manufacturer/Android version/display dimensions, app build identity and explicit problem notes. Not recorded: passwords, typed contract values, device identifiers/name, imported file names/paths/contents, original market data, screenshots, screen recording or other-app activity. Memory samples occur while the panel is loaded and recording; they are not full process-memory profiling. Pre-opt-in startup, hard crashes, freezes and final queued events are not guaranteed to be captured. Limits: 2000 event attempts, 256 pending entries and 2 MiB event file; omissions/storage issues are reported.

Development-signed APK, separate app ID com.quantforge.diagnostic. Future test signing keys may change; export diagnostics before uninstalling an older test version if Android rejects an update signature. Do not override an unexplained integrity warning. No live-account functionality is enabled. This explicit early diagnostic-test approval is an exception to the normal completed-usable-APK release gate.


## v30.07 — File declaration consistency after phone feedback

Minute inspection and comparison now require a standard NT8 filename such as MES 09-26.Last.txt. The declared contract/month/year and selected Last/Bid/Ask must match that filename. Mismatches and unrecognized names are rejected before reading. Keep original export names; do not rename a mismatched file merely to bypass this check. Matching labels cannot prove what is inside a file. Contents identity, independent provenance, coverage and benchmark validation remain unfinished; research stays disabled.

Diagnostics now distinguish total user action duration (including the picker) from DataProcessing/ComparisonProcessing duration measured after picker return. Processing duration includes opening the provider stream, reading/parsing and comparison; it is not pure computation time. No original filename or declared text is added to the diagnostic log. App version 0.30.7/code 3007; export your existing logs before changing test installs.


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


## v30.10b — Reject unavailable picker entries

v30.10a source 970c8a6c5109dde7a890d4e8502fb79d5d4ba964 is FAILED / NON-STABLE: PR run 36223880051 passed static and 235 core plus 23 reflection-disabled checks, then Android exposed CS8602 at MainPage.cs:129 because MAUI annotates picker entries as nullable. The Switch ambiguity is resolved. Windows validation was still running when this correction was prepared; no success is inferred. No APK was delivered.

v30.10b checks each entry and rejects the batch through the existing unavailable-provider path before constructing a stream source if an entry is null. It does not silently discard entries or publish partial results. Same authorized Phase 5 scope, branch and app version 0.30.10/code3010; new source SHA distinguishes the correction. Modified the same five files as v30.10a; no new/deleted files. Commit: v30.10b: reject nullable picker entries before batch source creation. Exact-head native/core/static/package validation pending; v30.09 remains the latest fully validated diagnostic baseline. Progress estimate stays 38% for Phase 5; device validation and end-to-end research remain unfinished. Main merge/stable promotion/live authority unchanged and unapproved. No new approval queue.

## v30.12b publication artifacts
Complete research runs can be assembled into deterministic publication artifacts. The artifact binds the job, dataset, evidence and execution-trace fingerprints to exported chart and ledger data. Incomplete or data-blocked runs cannot be published or retained in the result archive.

## v30.13 workflow packages and local result storage
A workflow package binds its research summary to the complete publication artifacts produced by the workflow. Validated publications may be persisted locally by artifact fingerprint. The store uses a manifest collision check and atomic directory publication; blocked or invalid research cannot be persisted as a publication.

## v30.14 end-to-end coordination and catalog persistence
End-to-end research requires both a catalog-admitted dataset and an admitted sanitized strategy envelope. Dataset catalog entries can be persisted and reloaded locally; reload does not weaken the existing admission rules.

## Optimization result review and recovery (v30.24)

Optimization results are read-only research artifacts. A completed optimization result is shown only after its deterministic selection is revalidated against all completed variants. The application may display final simulated equity and realized P&L for the evaluated variants, but it cannot submit orders, obtain live-account authority, or modify application settings from an optimization result.

Persisted optimization results use paired JSON and text representations. Recovery verifies the storage key, read-only authority flags, variant identities, selection consistency, and agreement between both representations. Missing, incomplete, or tampered result storage is rejected rather than partially recovered.

## Walk-forward research (v30.25)

Walk-forward research is represented as explicit temporal segments. Each segment declares a training window followed by a non-overlapping evaluation window. Optimization inputs are restricted to the training window. After deterministic selection, the out-of-sample evaluation must use the selected strategy and parameter fingerprints unchanged. Evaluation is still subject to dataset catalog admission, research-safe strategy admission, data reliability, and authoritative session coverage.

QuantForge does not use future evaluation observations to select the training winner. A segment whose optimization cannot produce a complete selection, or whose evaluation cannot complete, blocks the walk-forward result rather than filling or inferring missing performance.

## Walk-forward result review and recovery (v30.26)

Completed walk-forward runs can be presented as read-only segment results showing the selected training job and the later out-of-sample evaluation state. QuantForge verifies the segment fingerprints and the whole walk-forward result fingerprint before exposing those results.

Walk-forward outcomes may be persisted as paired JSON and Markdown artifacts. On recovery, both representations must agree and all nested optimization/research invariants are rechecked. Tampered or incomplete storage is rejected rather than partially trusted.

## Walk-forward application workflow (v30.27)

The application can run a validated walk-forward plan, persist the resulting evidence, and present the resulting state through one read-only workflow. Recovery performs the same integrity checks before presentation. If persisted evidence later fails validation, the presentation session clears its prior snapshot and reports a stable rejected diagnostic rather than continuing to display stale results.

## Android admission-readiness view (v30.34)

After a mixed TXT/ZIP batch inspection completes, QuantForge now shows an **Admission readiness** section for each inspected source. This section is explanatory only. It tells you whether the source is blocked or which independently trusted evidence is still required, such as an external dataset ID, independent instrument/timeframe identity, immutable provenance, or conflict resolution.

A source shown as **Ready** is only ready to enter the existing admission-request pipeline. It is not automatically admitted, it does not prove exchange-session completeness, and it does not enable research or live trading. Filename labels, matching prices, and cross-source agreement remain descriptive evidence rather than trusted provenance.

## External admission evidence on Android (v30.35)

After a mixed batch has completed, use **Check external admission evidence JSON** to inspect a QuantForge admission-evidence document. The document must independently provide the dataset ID, instrument identity, timeframe identity, and immutable provenance. QuantForge matches the evidence's sanitized-artifact fingerprint to exactly one admission-eligible inspected source and then runs the existing admission validation as a dry run.

A successful message means **an admission request can be formed**. The screen does not write the dataset catalog or grant research authority. If the evidence fingerprint does not match the inspected bytes, the instrument conflicts with the descriptive filename label, the evidence is malformed/ambiguous, or no eligible source matches, the operation fails closed.

## Registering a verified dataset on Android (v30.36)

1. Run the mixed TXT/ZIP batch inspection.
2. Use **Check external admission evidence JSON** and select independently prepared evidence whose sanitized-artifact SHA-256 matches exactly one admission-eligible inspected source.
3. Review the dry-run result. If the evidence and inspected bytes agree, **Register verified dataset in local catalog** becomes available.
4. Tap the registration button only when you intend to persist that exact dataset identity locally. QuantForge reruns the existing fail-closed admission checks before writing the catalog.
5. Use **Show admitted datasets** to reload and review the local catalog.

Registration does not authorize strategy execution or live trading. Research still requires the separate strategy, reliability, session-coverage and workflow gates. A filename, matching prices, or cross-source agreement never substitutes for independently supplied identity/provenance. If persistent catalog data fails validation, the catalog view rejects it rather than displaying partially trusted entries.

## Authoritative session coverage on Android (v30.37)

After admitting a minute dataset into the local catalog, keep or re-inspect the exact admitted market-data bytes in the current batch. Then use **Check authoritative session coverage JSON** and choose independently supplied session-policy evidence for that dataset.

QuantForge requires the evidence dataset ID and SHA-256 to match exactly one validated catalog entry and exactly one current inspected minute source. The policy must contain explicit UTC session intervals, policy identity/version, provenance fingerprint, and `authoritative: true`. QuantForge recomputes coverage from the inspected bars and displays expected minutes, observed in-session minutes, missing minutes, outside-session minutes and a deterministic policy fingerprint.

The evidence file does not carry a trusted `coveragePassed` outcome. Complete authoritative coverage satisfies only the session-coverage gate for those exact bytes. Research still requires the separate dataset, strategy, reliability and workflow gates, and live/order authority remains disabled.


## Research evidence workspace (v30.44)

The research pipeline now has explicit persisted evidence boundaries from admission through terminal outcomes. A research gate bundle combines one admitted dataset, authoritative complete session coverage, research-admissible reliability evidence, and one admitted research-safe strategy. A launch intent then binds that gate bundle to an exact research job identity before execution. Terminal reports can be persisted only as outcomes tied back to that launch identity.

QuantForge can compare two terminal outcomes for the same admitted dataset side by side. This comparison is descriptive: it records identities, terminal status, and simulated equity difference only when both reports are complete. It does not declare a strategy winner and cannot promote a strategy to live authority.

A canonical research workspace index can reference validated gate bundles, launch intents, outcomes and comparisons. The index is deterministically ordered and fingerprinted, rejects duplicates and unknown entry types, and is revalidated on load. It is an evidence-navigation layer, not an authority layer.

## Android strategy drafting and research-session preview (v30.79b-v30.84a)

QuantForge now persists the consolidated program-readiness report, supports canonical research-only strategy drafts with deterministically ordered parameters, reviews those drafts against already-admitted sanitized strategy evidence, and exposes the resulting readiness as an Android-facing strategy draft card. Drafts never gain order-submission authority. A mismatch between draft strategy identity and admitted strategy evidence remains blocked.

A ready strategy draft can participate in a read-only Android launch preview only when the exact selected dataset, selected admitted strategy, and research-readiness evidence agree. The preview can prepare a simulation-only research run; it cannot submit orders. Android research session summaries bind the launch preview to queue state and, when available, the terminal result identities. Session summaries are fingerprinted and can be persisted/recovered without converting UI state into execution authority.

The Android candidate version associated with this release line is 0.30.84 / code 3084. An APK must be built from this exact source revision before device testing; use the included GitHub build instructions.


## v30.91b AutoMellon 15-iteration boundary

Fifteen locally validated research-only iterations from v30.84b through v30.91b add deterministic Android research recovery/timeline/export evidence, strategy and launch guard summaries, run/outcome/comparison digests, workspace health, exact-source native validation request/receipt contracts, explicit native-evidence promotion gating, and self-verifying handoff/release-boundary evidence. App candidate 0.30.91/code3091. Static architecture and deterministic package-source gates pass. Native Android/Windows build and device validation remain external requirements; no stable promotion or live/order authority is implied.
