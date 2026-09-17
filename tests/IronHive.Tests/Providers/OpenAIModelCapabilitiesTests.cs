using System.ClientModel.Primitives;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI;
using OpenAI.Responses;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Responses generator translates the requested reasoning effort into a value the target model accepts.
/// The accepted sets are the vendor's own validation messages (2026-09-17): gpt-4 models take no
/// reasoning.effort at all, gpt-5 takes minimal..high, gpt-5.1 none..high, gpt-5.4/5.5 none..xhigh,
/// gpt-5.6 adds max, the o-series takes low..high. Sending anything outside the set is a 400.
/// </summary>
public class OpenAIModelCapabilitiesTests
{
    private static string? WireEffort(string model, MessageThinkingEffort? effort,
        IReadOnlyDictionary<string, OpenAIModelCapabilities>? overrides = null)
    {
        var options = OpenAIMessageGenerator.BuildOptions(
            new MessageGenerationRequest { Model = model, ThinkingEffort = effort, Messages = [Message.User("Hi")] },
            overrides);
        if (options.ReasoningOptions is null)
            return null;
        using var doc = System.Text.Json.JsonDocument.Parse(ModelReaderWriter.Write(options).ToString());
        return doc.RootElement.GetProperty("reasoning").GetProperty("effort").GetString();
    }

    [Theory]
    // None is "off": none where the model has it, otherwise its lowest value.
    [InlineData("gpt-5.5", MessageThinkingEffort.None, "none")]
    [InlineData("gpt-5.1-2025-11-13", MessageThinkingEffort.None, "none")]
    [InlineData("gpt-5-mini", MessageThinkingEffort.None, "minimal")]
    [InlineData("o4-mini", MessageThinkingEffort.None, "low")]
    // Minimal is rejected from gpt-5.1 on — the nearest thinking value is low.
    [InlineData("gpt-5.1", MessageThinkingEffort.Minimal, "low")]
    [InlineData("gpt-5.4-mini", MessageThinkingEffort.Minimal, "low")]
    [InlineData("gpt-5-nano", MessageThinkingEffort.Minimal, "minimal")]
    // XHigh is rejected by gpt-5, gpt-5.1 and the o-series — high is the ceiling there.
    [InlineData("gpt-5", MessageThinkingEffort.XHigh, "high")]
    [InlineData("gpt-5.1", MessageThinkingEffort.XHigh, "high")]
    [InlineData("o3", MessageThinkingEffort.XHigh, "high")]
    [InlineData("gpt-5.5", MessageThinkingEffort.XHigh, "xhigh")]
    [InlineData("gpt-5.6-luna", MessageThinkingEffort.XHigh, "xhigh")]
    // A model newer than the table is assumed to be the latest generation.
    [InlineData("gpt-7", MessageThinkingEffort.Medium, "medium")]
    public void Effort_IsAValueTheModelAccepts(string model, MessageThinkingEffort effort, string expected)
    {
        WireEffort(model, effort).Should().Be(expected);
    }

    [Theory]
    [InlineData("gpt-4.1-mini", MessageThinkingEffort.High)]
    [InlineData("gpt-4o", MessageThinkingEffort.None)]
    public void Effort_OnAModelWithoutReasoning_IsNotSent(string model, MessageThinkingEffort effort)
    {
        WireEffort(model, effort).Should().BeNull("'reasoning.effort' is not supported with this model");
    }

    [Fact]
    public void UnsetEffort_IsNotSent()
    {
        WireEffort("gpt-5.5", null).Should().BeNull("unset means the model's own default");
    }

    [Fact]
    public void NoneEffort_DoesNotAskForEncryptedReasoning()
    {
        var options = OpenAIMessageGenerator.BuildOptions(
            new MessageGenerationRequest { Model = "gpt-5.5", ThinkingEffort = MessageThinkingEffort.None, Messages = [Message.User("Hi")] },
            null);

        options.IncludedProperties.Should().BeEmpty();
    }

    [Fact]
    public void Override_WinsOverBuiltIn()
    {
        var overrides = new Dictionary<string, OpenAIModelCapabilities>
        {
            ["gpt-4.1"] = new() { ReasoningEfforts = ["low", "high"] },
        };

        WireEffort("gpt-4.1-mini", MessageThinkingEffort.Medium, overrides).Should().Be("high", "ties go to the higher value");
    }
}
