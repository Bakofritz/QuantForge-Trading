namespace QuantForge.Core;

public sealed class ProductApplicationCoordinator
{
    public ProductApplicationViewModel Present(
        ResearchWorkflowSummary summary,
        ProductWorkspaceSection activeSection)
    {
        var workflowState = ProductUiBoundary.CreateReadOnlyState(summary);
        return ProductApplicationViewModelRules.Create(workflowState, activeSection);
    }

    public ProductApplicationViewModel PresentWorkflowResult(
        ResearchWorkflowResult result,
        ResearchBatchMode mode,
        ProductWorkspaceSection activeSection)
    {
        ArgumentNullException.ThrowIfNull(result);
        var summary = ResearchWorkflowSummaryFactory.Create(mode, result);
        return Present(summary, activeSection);
    }

    public ResearchAuthorityDecision ValidateCommand(
        ProductApplicationViewModel state,
        ProductUiCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.LiveAccountEnabled || state.CanSubmitOrders || state.CanChangeApplicationSettings)
            throw new InvalidOperationException("Product application command validation received an authority-escalated view model.");

        if (state.AllowedOperations is null || !state.AllowedOperations.Contains(command.Operation))
            throw new InvalidOperationException("The requested product operation is not available in the active workspace section.");

        if (!state.ResearchCommandsEnabled &&
            command.Operation is ProductUiOperation.StartResearch or ProductUiOperation.StartOptimization)
            throw new InvalidOperationException("Research execution remains disabled while data reliability is blocking admission.");

        return ProductUiBoundary.RequireResearchOnly(command);
    }
}
