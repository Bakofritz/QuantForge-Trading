# QuantForge v26.48 Failed Validation Report

GitHub Actions run: `36067507767`

Result: FAIL

- `static-contract-gate`: success
- `dotnet-tests / Restore`: success
- `dotnet-tests / Build`: failure
- `dotnet-tests / Test`: skipped

Compiler failure:

`tests/QuantForge.Core.ArchitectureTests/ResearchRunnerTests.cs(144,31): error CS8629: Nullable value type may be null.`

The failure is a test-project nullability compile error. The core library itself compiled before the test-project error stopped the build.

Local native compilation is not claimed.
