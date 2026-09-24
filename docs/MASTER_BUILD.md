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
- Added regression tests for deterministic fill arithmetic and evidence-chain divergence.
- No live order route or broker integration was introduced.

### Validation status
- Static architecture review: completed.
- Native .NET compile/test: not locally available.
- GitHub Actions native validation: configured; run status must be observed before claiming pass.
- No trading performance numbers were generated.

## v26.45 — Account-state and report contracts
Next stability target:
- isolated simulated account state
- realized/unrealized P&L ledger semantics
- report schema with explicit data-blocked state
- reproducible research result package

## Progress estimate after v26.44
- Core research/simulation: 84%
- Safety/governance: 92%
- Strategy quarantine/scrubbing/import: 86%
- Historical-data integrity/provenance: 87%
- Read-only multi-strategy research: 84%
- Optimization/research admission: 79%
- Native Android/Windows production: 45–50%
- Full end-to-end implementation: 66–70%
- Overall usable research platform: 80–84%

These are engineering planning estimates, not runtime certification.
