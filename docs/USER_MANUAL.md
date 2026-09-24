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

The current v26.45 account contract tracks:
- cash
- long position quantity
- average entry price
- realized P&L
- unrealized P&L at a supplied market price
- equity

A sell cannot exceed the simulated long position. Short-position accounting is not implicitly enabled by this contract.

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

## Provenance

An imported artifact should retain:
- source URI
- retrieval time
- SHA-256
- original fingerprint
- sanitized fingerprint
- scrub-report identity

## Current implementation status

The native .NET source and architecture tests are being built incrementally. GitHub Actions is the authoritative native build/test environment for this stage.

A green test workflow is evidence for the tested source revision only; it is not a claim of completed Android/Windows product functionality.

## Troubleshooting

Import blocked: inspect the capability inventory and feature-selection result.

Research job blocked: inspect the first failed admission gate.

Backtest blocked: verify data admission, temporal partition, and execution timing.

Optimization has no performance: verify that authoritative data bytes were actually admitted. A data-blocked result is expected when source coverage is incomplete.

CI not green: do not treat the presence of tests as proof of a passing build; use the workflow result.
