using System.Reflection;
using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using Xunit;

namespace IronHive.Tests.Agent;

/// <summary>
/// Every knob <see cref="AgentParametersConfig"/> exposes must have somewhere to go.
///
/// This is the teeth for the defect class found in 0.15.0: commit 1b38998 folded the request
/// parameter model and carried only MaxTokens forward, but AgentParametersConfig kept advertising
/// five knobs and the TOML parser kept parsing them. The result was silent no-op configuration —
/// no exception, no warning, no log line. A config surface that outlives its sink is a lie told to
/// the consumer, so the sink is asserted structurally rather than left to review.
/// </summary>
public class AgentParametersConfigSinkTests
{
    [Fact]
    public void Every_AgentParametersConfig_Property_Has_A_MessageRequest_Sink()
    {
        var configProperties = typeof(AgentParametersConfig)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name);

        var requestProperties = typeof(MessageRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var name in configProperties)
        {
            requestProperties.Should().Contain(name,
                $"AgentParametersConfig.{name} is parsed from agent TOML, so MessageRequest must have " +
                "somewhere to put it — otherwise the setting is a silent no-op for every consumer");
        }
    }

    [Fact]
    public void Every_AgentParametersConfig_Property_Survives_To_The_GenerationRequest()
    {
        var configProperties = typeof(AgentParametersConfig)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name);

        var generationProperties = typeof(MessageGenerationRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var name in configProperties)
        {
            generationProperties.Should().Contain(name,
                $"AgentParametersConfig.{name} must reach the provider, and MessageGenerationRequest " +
                "is the last hop before the provider adapters");
        }
    }
}
