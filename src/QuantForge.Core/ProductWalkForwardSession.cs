namespace QuantForge.Core;

public enum ProductWalkForwardSessionStatus
{
    AwaitingResult,
    Ready,
    Blocked,
    Invalid
}

/// <summary>Read-only presentation lifecycle for persisted walk-forward evidence.</summary>
public sealed class ProductWalkForwardSession
{
    public ProductWalkForwardSessionStatus Status { get; private set; } = ProductWalkForwardSessionStatus.AwaitingResult;
    public ProductUiWalkForwardState? State { get; private set; }
    public string DiagnosticCode { get; private set; } = "QF-WF-AWAITING-RESULT";

    public void Load(string rootDirectory, string resultFingerprint)
    {
        State = null;
        Status = ProductWalkForwardSessionStatus.Invalid;
        DiagnosticCode = "QF-WF-PRESENTATION-REJECTED";
        try
        {
            var recovered = WalkForwardApplicationWorkflow.Recover(rootDirectory, resultFingerprint);
            State = recovered.State;
            Status = recovered.State.Complete ? ProductWalkForwardSessionStatus.Ready : ProductWalkForwardSessionStatus.Blocked;
            DiagnosticCode = recovered.State.Complete ? "QF-WF-PRESENTATION-READY" : "QF-WF-RESULT-BLOCKED";
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or IOException)
        {
            // Keep a stable diagnostic and no stale state. Raw path/storage details are not exposed here.
        }
    }
}
