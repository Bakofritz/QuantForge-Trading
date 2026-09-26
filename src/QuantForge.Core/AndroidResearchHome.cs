namespace QuantForge.Core;
public sealed record AndroidResearchHome(string WorkspaceFingerprint,bool Consistent,int DatasetCount,int ReadyGateCount,int OutcomeCount,int BlockingAlertCount,bool CanPrepareResearch,bool LiveTradingEnabled,bool OrderSubmissionEnabled);
public static class AndroidResearchHomeRules
{public static AndroidResearchHome Create(ResearchWorkspaceIndex index){var d=ProductResearchWorkspaceDashboardRules.Create(index);var alerts=ResearchWorkspaceAlertRules.Create(index);var actions=ResearchActionAvailabilityRules.Create(d);return new(d.WorkspaceFingerprint,d.Consistent,d.Datasets,d.Gates,d.Outcomes,alerts.Count(x=>x.Severity==ResearchAlertSeverity.Blocking),actions.CanPrepareLaunch,false,false);}}
