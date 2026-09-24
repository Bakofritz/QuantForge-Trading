# QuantForge Architecture

## Layered research path

Market source → fingerprint → structural validation → data admission → temporal partition → strategy quarantine → capability scrub → user-selected feature set → strategy admission → execution timing policy → isolated research job → simulation/optimization → evidence ledger → report.

## Authority domains

1. Historical research
2. Simulated replay
3. Simulated account
4. Read-only research
5. Live account — separate and restricted

Research contracts always return no order-submission authority and no application-setting authority.

## Causal integrity

At observation time T, only information timestamped at or before T is admissible. Unfinished higher-timeframe bars are not treated as observed information.

## Execution timing

Signals must not silently receive same-bar fills. Default research timing is next-bar-open or next-eligible-tick. Same-bar execution requires an explicit close-auction model.

## Provenance

A publishable research result is bound to dataset fingerprint, strategy fingerprint, execution-policy fingerprint, parameter-set fingerprint, temporal partition, and research-job fingerprint.
