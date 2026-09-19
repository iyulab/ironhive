using System.Reflection;
using AwesomeAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

/// <summary>
/// Every option an orchestrator reads has to be settable through the builder that constructs it. The Handoff and
/// GroupChat builders copied options field by field into a fresh options object and dropped five of them
/// (StopOnAgentFailure, AgentMiddlewares, ContextScope, ResultDistiller, ResultDistillationOptions): a caller using
/// the builder could not get a middleware or a non-stopping run, and nothing said so. The builders follow a
/// Set{Property} convention; this roster holds them to it for every settable property of the base options.
/// </summary>
public class OrchestratorBuilderOptionRosterTests
{
    public static TheoryData<Type> Builders => new() { typeof(HandoffOrchestratorBuilder), typeof(GroupChatOrchestratorBuilder) };

    [Theory]
    [MemberData(nameof(Builders))]
    public void EveryBaseOption_HasASetterOnTheBuilder(Type builder)
    {
        var options = typeof(OrchestratorOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.DeclaringType == typeof(OrchestratorOptions))
            .Select(p => p.Name);
        var setters = builder.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).ToHashSet();

        options.Where(name => !setters.Contains("Set" + name)).Should().BeEmpty(
            $"{builder.Name} builds its orchestrator's options itself, so an option without a setter can never be set");
    }
}
