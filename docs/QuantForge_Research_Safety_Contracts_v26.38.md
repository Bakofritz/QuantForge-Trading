# QuantForge Research Safety Contracts — v26.38

## Import contract
Quarantine → fingerprint → static scrub → feature inventory → authority classification → user component selection → sanitized adapter → regression validation → provenance registration → research admission.

## Execution contract
Imported strategies default to historical, replay, and simulated-account domains. Live-account execution is a separate authority boundary.

## Read-only optimization contract
A read-only optimization job may:
- read admitted market data;
- inspect approved strategy behavior;
- execute simulations;
- compare parameter sets;
- generate analysis and evidence.

A read-only optimization job may not:
- submit orders;
- enter, modify, or close live positions;
- change application settings;
- grant itself higher authority;
- bypass data-admission or provenance checks.

## Fail-closed behavior
Missing authority, invalid provenance, failed data validation, or ambiguous execution capability must block admission rather than silently downgrade into an unsafe mode.

## Evidence
Security/safety decisions should be append-only, attributable to the relevant job/artifact, and reproducible from immutable identifiers.
