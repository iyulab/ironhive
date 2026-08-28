using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using Xunit;

namespace IronHive.Tests.Agent;

/// <summary>
/// Sampling parameters (Temperature/TopP/TopK/StopSequences) must reach the provider.
///
/// Regression guard: commit 1b38998 ("simplify request parameter model", 2026-06-30) removed the
/// MessageGenerationParameters base class and folded only MaxTokens forward, silently dropping the
/// other four. AgentParametersConfig kept exposing them and the TOML parser kept reading them, so
/// they became a silent no-op — and IronHive.Flux's adapters stopped compiling against them.
/// </summary>
public class SamplingParameterFlowTests
{
    [Fact]
    public void MessageRequest_Carries_SamplingParameters()
    {
        var request = new MessageRequest
        {
            Provider = "test",
            Model = "test-model",
            Temperature = 0.3f,
            TopP = 0.85f,
            TopK = 40,
            StopSequences = ["</done>"],
        };

        request.Temperature.Should().Be(0.3f);
        request.TopP.Should().Be(0.85f);
        request.TopK.Should().Be(40);
        request.StopSequences.Should().ContainSingle().Which.Should().Be("</done>");
    }

    [Fact]
    public void GenerationRequest_Receives_SamplingParameters_From_MessageRequest()
    {
        var request = new MessageRequest
        {
            Provider = "test",
            Model = "test-model",
            Temperature = 0.3f,
            TopP = 0.85f,
            TopK = 40,
            StopSequences = ["</done>"],
        };

        var context = new MessageContext(request);

        context.Request.Temperature.Should().Be(0.3f,
            "a sampling parameter set on the request must survive the hop to the generation request");
        context.Request.TopP.Should().Be(0.85f);
        context.Request.TopK.Should().Be(40);
        context.Request.StopSequences.Should().ContainSingle().Which.Should().Be("</done>");
    }

    [Fact]
    public void AgentInvokeOptions_Can_Override_SamplingParameters_PerRequest()
    {
        var options = new AgentInvokeOptions
        {
            Temperature = 0.1f,
            TopP = 0.5f,
            TopK = 5,
            StopSequences = ["STOP"],
        };

        options.Temperature.Should().Be(0.1f);
        options.TopP.Should().Be(0.5f);
        options.TopK.Should().Be(5);
        options.StopSequences.Should().ContainSingle().Which.Should().Be("STOP");
    }
}
