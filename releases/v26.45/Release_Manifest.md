# v26.45 Release Manifest

Release: QuantForge v26.45
Branch: master/v26.45-native-stabilization
Baseline: 9ade2a335f3c5f07487faf68ab11e0018e195605

## Increment
Native build stabilization plus simulated-account and research-report contracts.

## Added
- Explicit xUnit global using for native test compilation.
- SimulationFill side identity.
- Isolated simulated-account cash, position, realized/unrealized P&L, and equity semantics.
- Reproducible research-report identity and explicit DataBlocked/Invalid states.
- Regression tests for account isolation, P&L, and report blocking.

## Explicit non-capabilities
- No live broker adapter.
- No live-account authority.
- No real-order submission path.
- No trading performance claim.

## Validation
Native validation is delegated to GitHub Actions. Final certification is determined only from the resulting workflow run.
