namespace QuantForge.Core;

public sealed class ResearchResultArchive
{
    private readonly Dictionary<string, ResearchPublicationArtifact> _artifacts = new(StringComparer.Ordinal);

    public int Count => _artifacts.Count;

    public void Add(ResearchPublicationArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ResearchPublicationArtifactRules.Validate(artifact);
        if (artifact.Publication.Report.Status != ResearchResultStatus.Complete)
            throw new InvalidOperationException("Only complete research publications may enter the result archive.");
        if (string.IsNullOrWhiteSpace(artifact.ArtifactFingerprint) || string.IsNullOrWhiteSpace(artifact.Manifest))
            throw new InvalidOperationException("Research publication identity is incomplete.");

        if (_artifacts.TryGetValue(artifact.ArtifactFingerprint, out var existing))
        {
            if (!string.Equals(existing.Manifest, artifact.Manifest, StringComparison.Ordinal))
                throw new InvalidOperationException("Publication fingerprint collision contains different content.");
            return;
        }

        _artifacts.Add(artifact.ArtifactFingerprint, artifact);
    }

    public bool TryGet(string artifactFingerprint, out ResearchPublicationArtifact? artifact) =>
        _artifacts.TryGetValue(artifactFingerprint, out artifact);

    public IReadOnlyList<ResearchPublicationArtifact> List() =>
        _artifacts.Values.OrderBy(x => x.ArtifactFingerprint, StringComparer.Ordinal).ToArray();
}
