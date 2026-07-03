using FluentAssertions;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// Regression guard for the 0.7.9 defect where the whole OpenAI family routed through the Responses API,
/// 404-ing every Chat-Completions-only endpoint. Compatible/GPUStack no longer go through a configurable
/// surface switch at all — they wire directly to <see cref="ChatCompletionMessageGenerator"/>, so this asserts
/// that wiring rather than a runtime flag that could regress back to the wrong default.
/// </summary>
public class OpenAICompatibleMessageGeneratorTests
{
    private static object GetInner(object generator) =>
        generator.GetType()
            .GetField("_inner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(generator)!;

    [Fact]
    public void OpenAICompatible_UsesChatCompletionsGenerator()
    {
        using var gen = new OpenAICompatibleMessageGenerator(new OpenAICompatibleConfig { BaseUrl = "http://localhost:11434", ApiKey = "k" });
        GetInner(gen).Should().BeOfType<ChatCompletionMessageGenerator>();
    }

    [Fact]
    public void GpuStack_UsesChatCompletionsGenerator()
    {
        using var gen = new GpuStackMessageGenerator(new GpuStackConfig { BaseUrl = "http://localhost:8080", ApiKey = "k" });
        GetInner(gen).Should().BeOfType<ChatCompletionMessageGenerator>();
    }
}
