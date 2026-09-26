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


## v30.10c local implementation note
The research data path now includes a fail-closed `DatasetCatalog` that binds exact dataset identity to structural validation, immutable provenance, and an inspection fingerprint before producing `DataAdmission`. This does not grant live authority or infer provenance from filenames/prices. Native compilation remains separately unverified in environments without the .NET SDK.

## Automellon v30.12b local implementation note
The result path now binds complete research publication artifacts to execution evidence and deterministic chart/ledger fingerprints. A local fingerprint-keyed result archive rejects incomplete publications and content collisions. This is research/simulation output only; live authority remains separate.

## Automellon v30.13a/v30.13b local implementation note
Research publications can now be grouped into a deterministic workflow package and persisted through a fingerprint-keyed local publication store. Persistence occurs only after complete publication validation and preserves the existing simulation-only boundary.

## Automellon v30.14a/v30.14b local implementation note
The research path now has an explicit end-to-end coordinator and persistent dataset catalog storage. Reloaded catalog entries are revalidated through the same immutable identity/provenance rules before admission.

## AutoMellon v30.34 Android admission-readiness note
Mixed market-data inspection now exposes a fail-closed admission-readiness explanation in the MAUI shell. The UI reports missing trusted identity/provenance inputs and blocking conflicts without manufacturing admission from filenames, prices, or comparison agreement. Android APK production remains exact-source only; use `docs/GITHUB_BUILD_INSTRUCTIONS.md` when building in a native-capable GitHub environment.

## AutoMellon v30.35 external evidence note
The Android/MAUI shell can now inspect a strict external market-admission evidence JSON and pair it to one completed inspected source by exact SHA-256. This is a dry-run bridge into the existing admission pipeline, not an automatic admission mechanism; no identity or provenance is inferred from market-data filenames or price agreement.

## AutoMellon v30.36 Android local catalog admission
The Android/MAUI shell now separates external-evidence verification from an explicit dataset-registration action. A verified request may be persisted into the local `DatasetCatalogFileStore`; exact duplicates are idempotent and conflicting immutable identities fail closed. The UI can reload and display admitted datasets for review. Catalog admission still does not authorize a strategy, research execution, broker connection, or live orders.

## AutoMellon v30.37 Android authoritative session evidence
The Android/MAUI shell can now inspect a strict authoritative session-policy JSON bound to an already admitted dataset and recompute session coverage from the exact currently inspected minute bars. QuantForge does not trust a precomputed pass/fail field from outside the application. Dataset ID/fingerprint, admitted catalog entry, current inspected bytes and explicit UTC policy must all agree before a coverage report is shown.


### v30.38b local candidate
Authoritative, complete session coverage is now persisted as a fingerprinted artifact on Android and reflected in a bounded research-readiness state. This advances an admitted dataset only to the remaining strategy/reliability/workflow gates; it does not grant research execution or live authority.


### v30.39a local candidate
Research-safe strategy admission can now be represented as an immutable fingerprinted artifact and persisted/recovered with identity validation. This does not create a strategy sanitizer or automatically admit arbitrary uploaded code; it preserves the existing requirement that an envelope already satisfy the research-safe admission contract before persistence.


### v30.44a local candidate
The ten-iteration AutoMellon cycle from v30.39b through v30.44a adds application-level admitted-strategy persistence, complete fingerprinted research-gate bundles, exact launch-intent persistence, terminal outcome persistence, deterministic side-by-side outcome comparison, and a canonical research-workspace index. These contracts preserve the existing fail-closed dataset/session/reliability/strategy gates and do not grant live, broker, order-submission, or settings authority. Native .NET/MAUI compilation remains separately unverified in the current local runtime.


## v30.91b AutoMellon 15-iteration boundary

Fifteen locally validated research-only iterations from v30.84b through v30.91b add deterministic Android research recovery/timeline/export evidence, strategy and launch guard summaries, run/outcome/comparison digests, workspace health, exact-source native validation request/receipt contracts, explicit native-evidence promotion gating, and self-verifying handoff/release-boundary evidence. App candidate 0.30.91/code3091. Static architecture and deterministic package-source gates pass. Native Android/Windows build and device validation remain external requirements; no stable promotion or live/order authority is implied.
