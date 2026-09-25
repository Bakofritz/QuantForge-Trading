# QuantForge Trading

QuantForge is a research-first trading platform under active master-build development.

## Repository policy

This repository is the canonical GitHub source for the QuantForge master build. The source tree is intentionally kept clean and cumulative: current implementation belongs under `src/`, tests under `tests/`, research artifacts under `research/`, and release artifacts under `releases/`.

Historical build ZIPs and manuals are retained only as immutable release evidence. They are not treated as source code.

## Safety boundary

Research approval, strategy scrubbing, optimization, replay, and simulated-account operation never grant live-account authority. Live-account operation is a separate authority domain.

Research execution is fail-closed when required data admission, provenance, causal integrity, capability classification, or execution timing information is missing or invalid.

## Current implementation status

The repository is in an incremental native-contract implementation phase. Native .NET compilation is not claimed until a .NET SDK-backed validation run succeeds.

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
