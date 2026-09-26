namespace QuantForge.Core;
public enum ResearchAlertSeverity{Info,Warning,Blocking}
public sealed record ResearchWorkspaceAlert(ResearchAlertSeverity Severity,string Code,string Message);
public static class ResearchWorkspaceAlertRules
{
 public static IReadOnlyList<ResearchWorkspaceAlert> Create(ResearchWorkspaceIndex index){var c=ResearchWorkspaceConsistencyRules.Validate(index);var list=new List<ResearchWorkspaceAlert>();if(!c.Valid)foreach(var d in c.Diagnostics)list.Add(new(ResearchAlertSeverity.Blocking,"workspace-consistency",d));if(c.GateEntries==0)list.Add(new(ResearchAlertSeverity.Warning,"no-gates","No admitted research gate evidence is indexed."));if(c.OutcomeEntries==0)list.Add(new(ResearchAlertSeverity.Info,"no-outcomes","No terminal research outcomes are indexed."));return list;}
}
