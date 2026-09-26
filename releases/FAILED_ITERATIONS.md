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


## v30.10a — Native Switch compile correction

v30.10 source c1c8a21c0a1160065ef82d78652b3e135d806089 is FAILED / NON-STABLE: PR Quality Gate 36210000549 passed static checks, 235 core tests, 23 reflection-disabled JSON checks and deterministic packaging, but both Android and Windows failed CS0104 at MainPage.cs:14 (Switch ambiguous between MAUI Controls and System.Diagnostics). No v30.10 APK was produced or delivered. The latest fully validated diagnostic baseline remains v30.09, 6ec5aaebd04d83437bdb0a29917126cbf37ec4aa; protected main stays at 8dbd746a7e09f41f9651a3df8c6970cf0d7e5322.

Correction explicitly qualifies Microsoft.Maui.Controls.Switch. This changes type resolution only. Version remains 0.30.10/code3010 because no candidate APK was delivered; source SHA identifies v30.10a. Exact-head native builds, core tests, static checks, source packaging and APK checks must pass before delivery. Continued correction is authorized within Phase 5 on master/v30.00-phase5-production-hardening / draft PR #9. No merge, stable promotion, Phase 6 or live authority.

Files modified: src/QuantForge.App/MainPage.cs; docs/MASTER_BUILD.md; docs/MELLON_MASTER_PROMPT.md; docs/USER_MANUAL.md; releases/FAILED_ITERATIONS.md. No added or removed files. Commit: v30.10a: disambiguate MAUI Switch and record failed native candidate. Phase 5 remains approximately 38% engineering estimate, conditional on native and device validation; overall usable/full-product completion and full-phase ETA remain unverified. Mixed Import Check remains the next device contribution; strategy samples and contributor activation remain deferred. Current hangups remain mixed-batch device validation, trusted data/session admission, persistent storage and executable strategy/simulation/chart/ledger workflow. No new approval queue.
