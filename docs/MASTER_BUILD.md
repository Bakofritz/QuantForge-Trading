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
- Source and repository integrity review: completed before modification.
- Native restore/build/test/static validation: pending GitHub Actions result for the v26.47 commit.
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
