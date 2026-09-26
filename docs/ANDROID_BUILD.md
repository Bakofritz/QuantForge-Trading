# QuantForge Android build contract

Android APKs must correspond to the exact committed source iteration. Do not reuse, rename, or relabel an APK produced from a different commit.

## Prerequisites

- .NET 10 SDK with the MAUI Android workload available.
- Android SDK configured through `ANDROID_SDK_ROOT` or `ANDROID_HOME`.
- Java supported by the installed .NET Android workload.
- Clean tracked Git worktree.

Check readiness:

```bash
python tools/build_android_apk.py --check
```

Build exact-source APK:

```bash
python tools/build_android_apk.py --output-dir artifacts/android
```

The build tool runs `dotnet publish` for `net10.0-android`, requires exactly one APK, copies it to a commit-bound filename, computes SHA-256, and writes `APK_BUILD_MANIFEST.json` plus a portable `.sha256` sidecar.

A source-only candidate must never be promoted to native-validated status merely because this tool exists. The APK must actually be produced and tested.

## Export pairing verification

Before an APK is placed in an AutoMellon export, verify that it belongs to the same exact source archive:

```bash
python tools/verify_android_apk_bundle.py \
  --source-zip QuantForge-vXX-source.zip \
  --apk QuantForge-<commit>-android.apk \
  --build-manifest APK_BUILD_MANIFEST.json
```

The verifier rejects mismatched commit/tree identity, renamed/relabelled APKs, hash mismatches, byte-count mismatches, and unexpected target frameworks. This check is required before an export may claim that an APK corresponds to its included source.

## GitHub/native helper prompts

See `docs/GITHUB_BUILD_INSTRUCTIONS.md`. These prompts never authorize building an older checkout under a newer release label.
