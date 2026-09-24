# v26.44 Release Manifest

Release: QuantForge v26.44
Branch: master/v26.41-clean-overhaul

## Increment
Deterministic fill and evidence layer.

## Added
- Deterministic next-eligible-event simulation fill.
- Explicit commission and slippage inputs.
- SHA-256 append-only evidence-chain contract.
- Regression tests.

## Explicit non-capabilities
- No live broker adapter.
- No live-account authority.
- No real-order submission path.
- No performance claim.

## Validation
Local native .NET compilation: unavailable in current environment.
CI native validation: configured; not yet certified from a completed workflow result.
