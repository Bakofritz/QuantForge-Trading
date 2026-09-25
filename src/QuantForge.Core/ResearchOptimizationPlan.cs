namespace QuantForge.Core;

public sealed record ResearchOptimizationPlan(
    IReadOnlyList<ResearchRunRequest> Variants);

public static class ResearchOptimizationPlanRules
{
    public static ResearchOptimizationPlan Create(IReadOnlyList<ResearchRunRequest> variants)
    {
        if (variants is null || variants.Count == 0)
            throw new InvalidOperationException("A research optimization plan requires at least one variant.");

        var jobFingerprints = new HashSet<string>(StringComparer.Ordinal);
        var variantKeys = new HashSet<string>(StringComparer.Ordinal);
        var ledgerNamespaces = new HashSet<string>(StringComparer.Ordinal);

        foreach (var variant in variants)
        {
            ResearchJobRules.RequireRunnable(variant.Job);

            if (variant.Intents is null || variant.Intents.Count == 0)
                throw new InvalidOperationException("Every optimization variant requires at least one simulation intent.");

            if (!jobFingerprints.Add(variant.Job.Identity.JobFingerprint))
                throw new InvalidOperationException("Optimization variants require unique research job fingerprints.");

            var identity = variant.Job.Identity;
            var variantKey = string.Join(
                "|",
                identity.DatasetFingerprint,
                identity.StrategyFingerprint,
                identity.ParameterSetFingerprint,
                identity.TemporalPartitionId);

            if (!variantKeys.Add(variantKey))
                throw new InvalidOperationException(
                    "Optimization variants must be unique by dataset, strategy, parameter set, and temporal partition.");

            var ledgerNamespace = variant.Intents[0].LedgerNamespace;
            if (string.IsNullOrWhiteSpace(ledgerNamespace) || !ledgerNamespaces.Add(ledgerNamespace))
                throw new InvalidOperationException("Each optimization variant requires an independent ledger namespace.");

            foreach (var intent in variant.Intents)
            {
                if (!string.Equals(intent.LedgerNamespace, ledgerNamespace, StringComparison.Ordinal))
                    throw new InvalidOperationException("All intents in one optimization variant must share its isolated ledger namespace.");
            }
        }

        return new ResearchOptimizationPlan(variants.ToArray());
    }
}
