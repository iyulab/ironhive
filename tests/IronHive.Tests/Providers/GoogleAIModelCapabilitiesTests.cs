using AwesomeAssertions;
using Google.GenAI.Types;
using IronHive.Abstractions.Messages;
using IronHive.Providers.GoogleAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// The Gemini generator translates the requested intent into what the target model accepts on the
/// wire, and GoogleAIModelCapabilities is the model-generation policy behind that translation. These
/// facts pin the translation without a network: a minimal thinking level on a model that rejects it
/// (Gemini 3.8 Flash, per the vendor model page) leaves as low; sampling parameters that model must
/// not receive are not forwarded; a Gemini 2.5 model is controlled by thinkingBudget rather than
/// thinkingLevel; and a consumer override wins over the built-in table.
/// </summary>
public class GoogleAIModelCapabilitiesTests
{
    private static GoogleAIMessageGenerator Generator(GoogleAIConfig? config = null)
        => new(config ?? new GoogleAIConfig { ApiKey = "test-key" });

    private static MessageGenerationRequest Request(string model, MessageThinkingEffort? effort = null, float? temperature = null)
        => new()
        {
            Model = model,
            ThinkingEffort = effort,
            Temperature = temperature,
            TopP = temperature is null ? null : 0.9f,
            TopK = temperature is null ? null : 40,
            Messages = [Message.User("Hi")],
        };

    [Fact]
    public void Resolve_LongestPrefixWins_AndUnknownModelIsDefault()
    {
        GoogleAIModelCapabilities.Resolve("gemini-3.8-flash-preview").SupportsMinimalThinking.Should().BeFalse();
        GoogleAIModelCapabilities.Resolve("gemini-2.5-flash").ThinkingControl.Should().Be(GoogleAIThinkingControl.Budget);
        GoogleAIModelCapabilities.Resolve("gemini-2.0-flash").ThinkingControl.Should().Be(GoogleAIThinkingControl.None);
        GoogleAIModelCapabilities.Resolve("gemini-3-pro").Should().BeSameAs(GoogleAIModelCapabilities.Default);
    }

    [Fact]
    public void MinimalThinking_OnGemini38Flash_LeavesAsLow()
    {
        var (_, config) = Generator().ToGoogleAIParams(Request("gemini-3.8-flash", MessageThinkingEffort.Minimal));

        config.ThinkingConfig!.ThinkingLevel.Should().Be(ThinkingLevel.Low, "minimal returns an error on this model");
        config.ThinkingConfig.ThinkingBudget.Should().BeNull();
    }

    [Fact]
    public void MinimalThinking_OnGemini3Pro_StaysMinimal()
    {
        var (_, config) = Generator().ToGoogleAIParams(Request("gemini-3-pro", MessageThinkingEffort.Minimal));

        config.ThinkingConfig!.ThinkingLevel.Should().Be(ThinkingLevel.Minimal);
    }

    [Fact]
    public void SamplingParameters_OnGemini38Flash_AreNotForwarded()
    {
        var (_, flash38) = Generator().ToGoogleAIParams(Request("gemini-3.8-flash", temperature: 0.2f));
        var (_, pro3) = Generator().ToGoogleAIParams(Request("gemini-3-pro", temperature: 0.2f));

        flash38.Temperature.Should().BeNull();
        flash38.TopP.Should().BeNull();
        flash38.TopK.Should().BeNull();
        pro3.Temperature.Should().Be(0.2f);
        pro3.TopP.Should().Be(0.9f);
        pro3.TopK.Should().Be(40);
    }

    [Fact]
    public void Thinking_OnGemini25_IsABudgetNotALevel()
    {
        var (_, config) = Generator().ToGoogleAIParams(Request("gemini-2.5-flash", MessageThinkingEffort.Low));

        config.ThinkingConfig!.ThinkingLevel.Should().BeNull();
        config.ThinkingConfig.ThinkingBudget.Should().Be(4_000);
    }

    [Fact]
    public void Thinking_OnGemini20_SendsNoThinkingConfig()
    {
        var (_, config) = Generator().ToGoogleAIParams(Request("gemini-2.0-flash", MessageThinkingEffort.Low));

        config.ThinkingConfig.Should().BeNull();
    }

    [Fact]
    public void Override_WinsOverBuiltIn()
    {
        var config = new GoogleAIConfig
        {
            ApiKey = "test-key",
            ModelCapabilities = new Dictionary<string, GoogleAIModelCapabilities>
            {
                ["gemini-3.8-flash"] = new() { SupportsMinimalThinking = true, SupportsSamplingParameters = true },
            },
        };

        var (_, generated) = Generator(config).ToGoogleAIParams(Request("gemini-3.8-flash", MessageThinkingEffort.Minimal, temperature: 0.2f));

        generated.ThinkingConfig!.ThinkingLevel.Should().Be(ThinkingLevel.Minimal);
        generated.Temperature.Should().Be(0.2f);
    }
}
