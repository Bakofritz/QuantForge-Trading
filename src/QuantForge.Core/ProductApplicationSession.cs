namespace QuantForge.Core;

public enum ProductSessionStatus
{
    AwaitingData,
    Ready,
    DataBlocked,
    Invalid
}

/// <summary>
/// Presentation lifecycle only. This is not a research admission token.
/// Call on the application UI thread; research services retain their own gates.
/// </summary>
public sealed class ProductApplicationSession
{
    private readonly ProductApplicationCoordinator _coordinator = new();

    public ProductSessionStatus Status { get; private set; } = ProductSessionStatus.AwaitingData;
    public ProductApplicationViewModel? State { get; private set; }
    public string DiagnosticCode { get; private set; } = "QF-AWAITING-DATA";
    public bool ResearchCommandsEnabled => State?.ResearchCommandsEnabled == true && Status == ProductSessionStatus.Ready;

    public void Load(ResearchWorkflowSummary? summary, ProductWorkspaceSection section)
    {
        // Clear the previous snapshot before validation so a rejected refresh
        // can never leave old results or command availability looking current.
        State = null;
        Status = ProductSessionStatus.Invalid;
        DiagnosticCode = "QF-PRESENTATION-REJECTED";

        try
        {
            ArgumentNullException.ThrowIfNull(summary);
            var candidate = _coordinator.Present(summary, section);
            State = candidate;
            Status = candidate.HasBlockingDataIssues ? ProductSessionStatus.DataBlocked : ProductSessionStatus.Ready;
            DiagnosticCode = candidate.HasBlockingDataIssues ? "QF-DATA-BLOCKED" : "QF-PRESENTATION-READY";
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException)
        {
            // Expose a stable diagnostic, never raw exception/path/secret text.
            // Unexpected failures propagate with the previous snapshot cleared.
        }
    }

    public ResearchAuthorityDecision ValidateCommand(ProductUiCommand command)
    {
        if (State is null || Status is ProductSessionStatus.AwaitingData or ProductSessionStatus.Invalid)
            throw new InvalidOperationException("No validated research presentation is loaded.");

        return _coordinator.ValidateCommand(State, command);
    }
}
