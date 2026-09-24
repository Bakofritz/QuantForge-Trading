# QuantForge Architecture Baseline — v26.37

## Purpose
Establish the repository-level architecture contract before adding executable implementation to the newly connected master repository.

## Native architecture
- Primary implementation target: C# / .NET 10 LTS.
- Application shell: .NET MAUI for Android and Windows.
- Shared libraries: Core domain, governance, backtesting, data, runtime/research, storage, AI gateway, Scout, reporting.
- Persistence: SQLite/local-first with immutable evidence and append-only event records.
- Optional Python worker: specialized numerical/scientific workloads only; not the primary application architecture.

## Authority domains
1. Historical research.
2. Simulated replay.
3. Simulated-account forward testing.
4. Read-only research/optimization.
5. Live-account operation — separately restricted and never implicitly enabled.

## Required boundary properties
- Research authorization cannot grant live execution authority.
- Read-only research cannot submit orders.
- Read-only research cannot change application settings.
- Imported code is quarantined before research registration.
- Data admission is upstream of research execution.
- Every publishable research result identifies its dataset, strategy, execution policy, parameter set, and temporal partition.

## Implementation status
This iteration establishes architecture/documentation contracts. It does not claim that the complete production application has been compiled or certified.
