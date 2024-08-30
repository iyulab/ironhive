namespace Raggle.Engines.Anthropic;

/// <summary>
/// <see href="https://docs.anthropic.com/en/api/getting-started">Anthropic API Documentation</see>
/// </summary>
internal class AnthropicConstants
{
    internal const string Host = "api.anthropic.com";
    internal const string AuthHeaderName = "x-api-key";
    internal const string VersionHeaderName = "anthropic-version";
    internal const string VersionHeaderValue = "2023-06-01";

    internal const string PostMessagesPath = "/v1/messages";

    /// <summary>
    /// <see href="https://docs.anthropic.com/en/docs/about-claude/models#model-names"/>
    /// </summary>
    internal static readonly IEnumerable<AnthropicModel> PredefinedChatModels =
    [
        new() { ModelId = "claude-3-5-sonnet-20240620" },
        new() { ModelId = "claude-3-opus-20240229" },
        new() { ModelId = "claude-3-sonnet-20240229" },
        new() { ModelId = "claude-3-haiku-20240307" },
        new() { ModelId = "claude-2.1" },
        new() { ModelId = "claude-2.0" },
        new() { ModelId = "claude-instant-1.2" },
    ];
}
