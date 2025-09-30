namespace IronHive.Providers.GoogleAI.Share;

/// <summary>
/// <see href="https://ai.google.dev/api">Google API Documentation</see>
/// </summary>
internal class GoogleAIConstants
{
    internal const string DefaultBaseUrl = "https://generativelanguage.googleapis.com/v1beta/";

    internal const string AuthorizationHeaderName = "x-goog-api-key";

    internal const string GetModelsListPath = "/models";
    internal const string PostGenerateContentPath = $"/models/{{0}}:generateContent";
    internal const string PostStreamGenerateContentPath = $"/models/{{0}}:streamGenerateContent";
    internal const string PostEmbedContentPath = $"/models/{{0}}:embedContent";
    internal const string PostBatchEmbedContentPath = $"/models/{{0}}:batchEmbedContent";
    internal const string PostCountTokensPath = $"/models/{{0}}:countTokens";
}