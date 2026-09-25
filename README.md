# QuantForge Trading

QuantForge is a futures-strategy research and controlled-simulation application built primarily in C# on .NET 10, with shared .NET MAUI application shells for Windows and Android. Its workflow takes market data and trading scripts through identification, validation, quarantine, capability auditing, user-selected feature filtering, and research admission before backtesting, simulated replay, or read-only optimization. The research core separates strategies and parameter variants into independent simulated ledgers, applies explicit execution timing, commission and slippage inputs, and binds results to dataset, strategy, parameter, job, and execution-evidence identities. Missing or unreliable data blocks research; no-forward-bias rules prevent future information or unfinished higher-timeframe bars from influencing earlier decisions.

The broader product goal combines candlestick charts, strategy drawings, detailed ledgers and reports, multi-timeframe research, simulated forward testing, AI oversight, script discovery, and bot coordination in the dark sunflower-style dashboard supplied as the UI reference. NinjaTrader data and C# scripts, TradingView Pine import/translation, and coordinated desktop/mobile use are target workflows whose complete end-to-end support still requires implementation and validation. The current repository provides tested research contracts and basic native research shells, not the finished visual dashboard or every planned import, AI, charting, and automation feature. Live-account access and broker order submission remain disabled and separately governed; successful auditing, simulation, optimization, or application builds never unlock them.

## Repository policy

This repository is the canonical GitHub source for the QuantForge master build. The source tree is intentionally kept clean and cumulative: current implementation belongs under `src/`, tests under `tests/`, research artifacts under `research/`, and release artifacts under `releases/`.

Historical build ZIPs and manuals are retained only as immutable release evidence. They are not treated as source code.

## Safety boundary

Research approval, strategy scrubbing, optimization, replay, and simulated-account operation never grant live-account authority. Live-account operation is a separate authority domain.

Research execution is fail-closed when required data admission, provenance, causal integrity, capability classification, or execution timing information is missing or invalid.

## Current implementation status

The Phase 4 baseline at `8dbd746a7e09f41f9651a3df8c6970cf0d7e5322` passed post-merge Quality Gate `36093570707`, including core tests and both native shell builds. Phase 5 hardening is active. Build success does not certify device runtime, finished UI, or production readiness.

See `docs/MASTER_BUILD.md` for the cumulative build ledger and current progress.

## Directory map

- `src/QuantForge.Core/` — core safety and research contracts
- `tests/` — architecture and regression tests
- `research/strategy-optimization/` — optimization specifications and reports
- `research/data/` — data manifests and provenance records
- `research/strategies/quarantine/` — imported source awaiting audit
- `docs/` — cumulative architecture, build, and user documentation
- `releases/` — immutable build artifacts
- `tools/` — deterministic validation utilities
- `.github/workflows/` — repository quality gates

## Development rule

Every master-build iteration must either add verified implementation value or improve reproducibility, safety, maintainability, documentation, or validation. No iteration may convert an unverified contract into a production claim.
