namespace IronHive.Providers.GoogleAI.Clients;

/// <summary>
/// <see href="https://ai.google.dev/api">Documentation</see>
/// </summary>
internal class GoogleAIConstants
{
    internal const string DefaultBaseUrl = "https://generativelanguage.googleapis.com/v1beta/";
    internal const string AuthorizationHeaderName = "x-goog-api-key";

    internal const string GetModelsListPath = "/models";
    internal const string PostGenerateContentPath = $"/{{0}}:generateContent";
    internal const string PostStreamGenerateContentPath = $"/{{0}}:streamGenerateContent";
    internal const string PostEmbedContentPath = $"/{{0}}:embedContent";
    internal const string PostBatchEmbedContentPath = $"/{{0}}:batchEmbedContents";
    internal const string PostCountTokensPath = $"/{{0}}:countTokens";
}