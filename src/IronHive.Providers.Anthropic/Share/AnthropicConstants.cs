namespace IronHive.Providers.Anthropic.Share;

/// <summary>
/// <see href="https://docs.anthropic.com/en/api/getting-started">Anthropic API Documentation</see>
/// </summary>
internal class AnthropicConstants
{
    internal const string DefaultBaseUrl = "https://api.anthropic.com/v1/";
    
    internal const string AuthorizationHeaderName = "x-api-key";
    internal const string VersionHeaderName = "anthropic-version";
    internal const string VersionHeaderValue = "2023-06-01";

    internal const string GetModelsPath = "/models";
    internal const string PostMessagesPath = "/messages";
}
