using FluentAssertions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// Regression guard for the 0.7.9 defect where the whole OpenAI family routed through the Responses API,
/// 404-ing every Chat-Completions-only endpoint. Compatible/GPUStack must select Chat Completions; first-party
/// OpenAI must keep Responses as its default.
/// </summary>
public class OpenAIApiSurfaceTests
{
    [Fact]
    public void OpenAIConfig_Default_TargetsResponses()
    {
        // first-party OpenAI keeps the Responses surface (reasoning summaries / encrypted content).
        new OpenAIConfig().Api.Should().Be(OpenAIApiSurface.Responses);
    }

    [Fact]
    public void OpenAICompatible_ToOpenAI_TargetsChatCompletions()
    {
        new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434" }
            .ToOpenAI().Api.Should().Be(OpenAIApiSurface.ChatCompletions);
    }

    [Fact]
    public void GpuStack_ToOpenAI_TargetsChatCompletions()
    {
        new GpuStackConfig { BaseUrl = "http://localhost:8080" }
            .ToOpenAI().Api.Should().Be(OpenAIApiSurface.ChatCompletions);
    }

    [Fact]
    public void Dispatcher_ChatCompletionsConfig_SelectsChatSurface()
    {
        using var gen = new OpenAIMessageGenerator(new OpenAIConfig
        {
            ApiKey = "k",
            Api = OpenAIApiSurface.ChatCompletions
        });
        gen.Surface.Should().Be(OpenAIApiSurface.ChatCompletions);
    }

    [Fact]
    public void Dispatcher_DefaultConfig_SelectsResponsesSurface()
    {
        using var gen = new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "k" });
        gen.Surface.Should().Be(OpenAIApiSurface.Responses);
    }

    [Fact]
    public void Dispatcher_ApiKeyCtor_DefaultsToResponses()
    {
        using var gen = new OpenAIMessageGenerator("k");
        gen.Surface.Should().Be(OpenAIApiSurface.Responses);
    }
}
