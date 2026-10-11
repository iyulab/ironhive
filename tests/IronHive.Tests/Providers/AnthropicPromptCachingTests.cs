using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;

namespace IronHive.Tests.Providers;

/// <summary>
/// The request carries one top-level <c>cache_control</c> (automatic caching) unless caching is off. Measured 2026-10-12
/// (claude-haiku-5-5, a ~17k-token system prompt, two streamed requests): the second read 17,115 cached tokens with
/// automatic caching and 0 with it off.
/// </summary>
public class AnthropicPromptCachingTests
{
    private static MessageGenerationRequest Request() => new()
    {
        Model = "claude-sonnet-5-5",
        Messages = [Message.User("Hi")],
    };

    private static string? CacheControlOf(AnthropicPromptCaching? caching)
    {
        var config = new AnthropicConfig { ApiKey = "test-key" };
        if (caching is { } c) config.PromptCaching = c;
        using var generator = new AnthropicMessageGenerator(config);
        var control = generator.ToMessageCreateParams(Request()).CacheControl;
        return control is null ? null : System.Text.Json.JsonSerializer.Serialize(control.RawData);
    }

    [Fact]
    public void Default_IsAutomaticCachingWithTheFiveMinuteLifetime()
    {
        CacheControlOf(null).Should().Be("""{"type":"ephemeral"}""");
        new AnthropicConfig().PromptCaching.Should().Be(AnthropicPromptCaching.FiveMinutes);
    }

    [Fact]
    public void OneHour_SendsTheOneHourLifetime()
        => CacheControlOf(AnthropicPromptCaching.OneHour).Should().Be("""{"type":"ephemeral","ttl":"1h"}""");

    [Fact]
    public void Off_SendsNoBreakpoint()
        => CacheControlOf(AnthropicPromptCaching.Off).Should().BeNull();

    [Fact]
    public void UndefinedValue_IsRefusedWhenTheGeneratorIsBuilt()
    {
        var act = () => new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "k", PromptCaching = (AnthropicPromptCaching)42 });
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
