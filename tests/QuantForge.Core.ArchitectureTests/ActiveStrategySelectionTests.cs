using QuantForge.Core;
namespace QuantForge.Core.ArchitectureTests;
public sealed class ActiveStrategySelectionTests
{[Fact] public void Selection_is_bound_to_admitted_artifact(){var a=TestFixtures.Gates().Strategy;var s=ActiveStrategySelectionRules.Create(a);ActiveStrategySelectionRules.Validate(s,a);}}
