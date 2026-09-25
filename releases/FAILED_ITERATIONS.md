# QuantForge Failed Iteration Catalog

Failed iterations are cataloged separately from confirmed stable builds and from correction work. A failed iteration is retained as historical engineering evidence and is never presented as a stable or operational release.

## v26.48 — Native build failure

- Branch: `master/v26.48-execution-evidence`
- Final failing commit: `815160dd454b0e29e337fce342a0e993cf3be259`
- GitHub Actions run: `36067507767`
- Static contract gate: PASS
- .NET restore: PASS
- Native build: FAIL
- Tests: SKIPPED because the build failed
- Compiler error: `ResearchRunnerTests.cs(144,31): CS8629: Nullable value type may be null`
- Status: FAILED / NON-STABLE
- Superseding correction: v26.49

The v26.48 source remains preserved on its Git branch for exact historical reference. Its failure record is separate from the stable release records under `releases/v26.46/` and `releases/v26.47/`.
