# v26.42 Release Manifest

Release: QuantForge v26.42
Branch: master/v26.41-clean-overhaul

## Increment
Data/strategy admission hardening.

## Added
- ResearchManifest canonical serialization/validation.
- StrategyFeatureSelection research-safe component boundary.
- ProvenanceRecord contract.
- Regression tests for manifest and feature selection.

## Safety
LiveAccount cannot be represented by a research manifest. Research feature selection cannot enable order submission or application-setting mutation.

## Validation
Local native .NET compilation: unavailable in current environment.
CI native validation: configured; not yet certified from a completed workflow result.
Trading performance: not evaluated.
