from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
required = [
    "README.md",
    "Directory.Build.props",
    "src/QuantForge.Core/QuantForge.Core.csproj",
    "src/QuantForge.Core/AuthorityBoundary.cs",
    "src/QuantForge.Core/CausalIntegrity.cs",
    "src/QuantForge.Core/ExecutionTimingPolicy.cs",
    "src/QuantForge.Core/StrategyCapabilityManifest.cs",
    "src/QuantForge.Core/DataAdmission.cs",
    "src/QuantForge.Core/ResearchJobIdentity.cs",
    "src/QuantForge.Core/ResearchJob.cs",
    "tests/QuantForge.Core.ArchitectureTests/AuthorityBoundaryTests.cs",
    "docs/MASTER_BUILD.md",
    "docs/USER_MANUAL.md",
]
missing = [p for p in required if not (ROOT / p).is_file()]
if missing:
    raise SystemExit("MISSING:" + ",".join(missing))

text = "\n".join((ROOT / p).read_text(encoding="utf-8") for p in required if p.endswith(".cs"))
assert "Live-account authority is outside the research runtime." in text
assert "Information timestamp cannot be after observation timestamp." in text
assert "Same-bar execution requires an explicit close-auction model." in text
assert "CanSubmitOrders: false" in text
assert "CanChangeApplicationSettings: false" in text
assert "DatasetFingerprint" in text and "StrategyFingerprint" in text
print("PASS: QuantForge repository static architecture gate")
