# QuantForge Master Build

## Operating rule

This repository is maintained as a cumulative master build. Iterations are incremental and are recorded here instead of producing redundant document families.

## v26.41 — Clean GitHub Directory Overhaul

### Objective
Convert the GitHub repository from a mixed release-drop layout into a maintainable source-of-truth layout.

### Changes
- Removed the prior loose `Resource Files/` and version-scattered documentation layout from the active source tree.
- Created explicit source, test, research, documentation, tools, release, and CI boundaries.
- Preserved v26.35 manual/build artifacts under an immutable release location rather than treating them as source.
- Added a native .NET core project boundary.
- Added fail-closed authority, causal-integrity, execution-timing, strategy-admission, data-admission, and research-job identity contracts.
- Added architecture tests for the new contracts.
- Added a repository-level README and ignore policy.

### Validation
- Source design reviewed for fail-closed semantics.
- Native .NET compilation remains unverified because the current build environment has no .NET SDK.
- Tests are present but are not represented as passed until a .NET SDK-backed run occurs.
- Historical v26.35 ZIP integrity was independently checked before retention.

## v26.42 — Data/strategy admission hardening

Planned as the next stable iteration: add canonical manifest schemas, provenance registration, and research-job serialization without granting live authority.

## v26.43 — Research execution skeleton

Planned: add deterministic simulation interfaces, ledger contracts, and isolated multi-strategy namespaces.

## Progress estimate after v26.41

- Core research/simulation: 78%
- Safety/governance: 90%
- Strategy quarantine/scrubbing/import: 84%
- Historical-data integrity/provenance: 84%
- Read-only multi-strategy research: 79%
- Optimization/research admission: 74%
- Native Android/Windows production: 45–50%
- Full end-to-end implementation: 60–64%
- Overall usable research platform: 75–79%

These are engineering planning estimates, not runtime certification.
