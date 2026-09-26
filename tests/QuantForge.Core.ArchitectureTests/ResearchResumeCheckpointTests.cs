using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ResearchResumeCheckpointTests
{[Fact] public void Terminal_stage_requires_outcome(){Assert.Throws<InvalidOperationException>(()=>ResearchResumeCheckpointRules.Create("w",ResearchResumeStage.TerminalOutcomeRecorded,"l"));}}
