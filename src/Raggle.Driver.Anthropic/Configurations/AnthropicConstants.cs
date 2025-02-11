namespace Raggle.Driver.Anthropic.Configurations;

/// <summary>
/// <see href="https://docs.anthropic.com/en/api/getting-started">Anthropic API Documentation</see>
/// </summary>
internal class AnthropicConstants
{
    internal const string DefaultEndPoint = "https://api.anthropic.com/";
    internal const string ApiKeyHeaderName = "x-api-key";
    internal const string VersionHeaderName = "anthropic-version";
    internal const string VersionHeaderValue = "2023-06-01";

    internal const string GetModelListPath = "/v1/models?limit=1000";
    internal const string PostMessagesPath = "/v1/messages";
}
