# QuantForge Research Pipeline — v26.39

## Canonical flow
Data source → fingerprint → structural validation → canonical data gate → temporal partition → strategy admission → execution policy → research job → simulation/optimization → evidence → report.

## Data gate
Every research path must pass through a canonical data gate before execution. External URLs are provenance references only; actual bytes must be retrieved, fingerprinted, validated, and admitted.

## Strategy gate
Only quarantined, scrubbed, user-reviewed, authority-classified strategy artifacts may enter research execution.

## Job isolation
Each research job receives an execution lease and an explicit authority domain. Research jobs must not inherit live-account authority.

## Multi-script operation
The pipeline supports one or many selected scripts in a read-only research domain. Each script remains independently identifiable so that results cannot be confused across strategies.

## Reproducibility identity
A publishable result should bind:
- dataset identity;
- strategy identity;
- execution-policy identity;
- parameter-set identity;
- temporal-partition identity;
- research-job identity.

## Current limitation
The repository is still being bootstrapped. These contracts define the intended implementation and validation surface; they are not a claim that every component is already executable.
