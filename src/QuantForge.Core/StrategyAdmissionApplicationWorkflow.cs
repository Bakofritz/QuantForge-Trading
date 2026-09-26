namespace QuantForge.Core;

public sealed record StrategyAdmissionCommitResult(
    StrategyAdmissionArtifact Artifact,
    string StoragePath);

/// <summary>
/// Application-facing persistence boundary for already-admitted research-safe strategy envelopes.
/// It never sanitizes or admits strategy code by itself.
/// </summary>
public sealed class StrategyAdmissionApplicationWorkflow
{
    private readonly StrategyAdmissionArtifactFileStore _store;

    public StrategyAdmissionApplicationWorkflow(StrategyAdmissionArtifactFileStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public StrategyAdmissionCommitResult Persist(StrategyAdmissionEnvelope envelope)
    {
        var artifact = StrategyAdmissionArtifactRules.Create(envelope);
        var path = _store.Save(artifact);
        var recovered = _store.Load(artifact.ArtifactFingerprint);
        StrategyAdmissionArtifactRules.Validate(recovered);
        if (recovered != artifact)
            throw new InvalidOperationException("Persisted strategy admission artifact does not round-trip exactly.");
        return new(artifact, path);
    }

    public StrategyAdmissionArtifact Recover(string artifactFingerprint) => _store.Load(artifactFingerprint);
}
