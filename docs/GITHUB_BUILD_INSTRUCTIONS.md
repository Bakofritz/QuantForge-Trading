# QuantForge GitHub/native build instructions

Use these prompts only after the GitHub checkout has been synchronized to the exact AutoMellon source iteration being tested. If the GitHub source is older than the supplied source ZIP/manifest, stop rather than building an older APK under a newer label.

## Android exact-source APK prompt

Act as the QuantForge Android build agent.

Repository: Bakofritz/QuantForge-Trading

Do NOT merge to protected main, change branch protection, enable live trading, or make unrelated source changes.

Goal: build a Release APK from the exact source currently checked out and produce downloadable build evidence.

1. First report:
   - current branch
   - HEAD commit SHA
   - git tree SHA
   - `git status --porcelain`
   - detected .NET SDK version
   - installed MAUI/Android workloads
   - Android SDK path
2. Refuse to build if tracked source has uncommitted modifications. Do not silently repair or alter source.
3. Locate the MAUI project that targets Android (`net10.0-android` or the Android target actually declared by the project). Do not guess a project filename.
4. Run the appropriate restore/workload restore, then perform a clean Release Android build/publish using the repository's declared target framework and version settings.

Prefer commands equivalent to:

```text
dotnet workload restore
dotnet restore
dotnet publish <detected-maui-project.csproj> -f net10.0-android -c Release
```

If the project requires repository-specific properties, derive them from the project/build files rather than inventing values.

5. Treat all compiler errors and warnings according to the repository's existing build policy. Do not suppress errors or weaken analyzers merely to obtain an APK.
6. Find the exact generated `.apk`. There must be exactly one intended test APK. If multiple APKs exist, explain them and select only the one corresponding to this Release build.
7. Compute SHA-256 for the APK.
8. Create `APK_BUILD_MANIFEST.json` containing at minimum UTC build timestamp, repository, branch, commit SHA, tree SHA, app version, Android version code, target framework, Release configuration, .NET SDK version, APK filename/size/SHA-256, signing classification, and exact build command.
9. Create a SHA-256 sidecar that uses only the APK filename, not an absolute or temporary filesystem path.
10. Package the APK, build manifest, hash sidecar, and concise build log into `QuantForge_<app-version>_<short-commit>_Android_Test_Build.zip`.
11. Return build pass/fail, commit SHA, app version/version code, APK filename/SHA-256, warnings/errors count, and downloadable artifact location.

Do not claim device acceptance, stable promotion, or production readiness. This is an exact-source Android test build only.

If the source available in GitHub is older than the source package I provide, STOP and tell me that exact-source synchronization is required instead of building an older APK.

## Follow-up prompt if GitHub needs one more instruction

Proceed with the exact-source Release Android build now. Make no unrelated source changes. Return the APK ZIP, SHA-256, commit/tree identity, app version/version code, and any build errors exactly as requested.

## Windows native test prompt

Act as the QuantForge Windows native test-build agent using the exact same source revision. Do not modify source merely to make the build pass. Report branch/commit/tree/dirty status and .NET workload state first. Restore, then build the declared Windows target in Release using the repository's project settings. Run the repository core/architecture tests and deterministic packaging checks. Return exact commands, warnings/errors, test totals, output artifact paths and SHA-256 hashes. Do not merge, promote stable, or enable any live/broker/order authority. If the checked-out source does not match the supplied release source identity, stop instead of testing an older revision.
