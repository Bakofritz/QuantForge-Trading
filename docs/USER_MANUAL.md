# QuantForge User Manual

## What QuantForge is

QuantForge is a research and controlled simulation platform. It is designed to audit imported strategies, validate market data, run causal backtests/replays, perform read-only optimization, and conduct simulated forward testing.

It does not silently convert research approval into live trading authority.

## First-run mental model

Think of QuantForge as a series of locked gates:

1. Data gate — Is the market data structurally valid and traceable?
2. Strategy gate — What can the imported code actually do?
3. Authority gate — What is the strategy allowed to do in this research job?
4. Causal gate — Could any information from the future reach the decision?
5. Execution gate — When is a signal actually allowed to become a simulated fill?
6. Evidence gate — Can the result be reproduced from immutable identities?

A failed gate blocks the research job.

## Importing a strategy

Imported code first enters quarantine. QuantForge records the original fingerprint, scans for capability categories, and creates an inventory for user review.

The scrubber does not grant live trading permission.

Research admission is intended for historical, replay, simulated-account, and read-only research domains.

## Backtesting and optimization

A valid research result requires identified data, explicit timeframe/session semantics, transaction costs, slippage/fill assumptions, strategy parameters, and temporal partitions.

Optimization is read-only. It may analyze one or many admitted strategies against approved market data, but it cannot enter positions or change application settings.

No fabricated performance metrics are acceptable. If authoritative data is unavailable, the result is data-blocked.

## Forward testing

Forward testing uses a simulated account and ledger. The simulated environment is isolated from live-account authority.

A current-bar signal cannot silently become a same-bar fill. Use next-bar-open, next-eligible-tick, or an explicitly modeled close auction.

## Results

Every publishable research result should identify:
- dataset and SHA-256 fingerprint
- strategy and SHA-256 fingerprint
- execution policy
- parameter set
- temporal partition
- research-job identity
- cost/slippage assumptions
- validation status

## Current limitation

Native .NET compilation and native Android/Windows runtime are not claimed until validated in an environment with the required SDK/toolchain.

## Troubleshooting

**Research job blocked:** inspect the first failed gate; do not bypass it.

**Strategy rejected:** review the capability inventory and research authority manifest.

**Backtest rejected:** verify data admission, temporal partition, and execution timing.

**Performance unavailable:** confirm that authoritative historical data has been admitted. QuantForge must report data-blocked status instead of inventing numbers.
