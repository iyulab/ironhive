namespace IronHive.Providers.OpenAI;

/// <summary>
/// <see href="https://platform.openai.com/docs/api-reference/introduction">Documentation</see>
/// </summary>
internal class OpenAIConstants
{
    internal const string DefaultBaseUrl = "https://api.openai.com/v1/";
    internal const string AuthorizationHeaderName = "Authorization";
    internal const string AuthorizationHeaderValue = $"Bearer {{0}}";
    internal const string OrganizationHeaderName = "OpenAI-Organization";
    internal const string ProjectHeaderName = "OpenAI-Project";

    internal const string GetModelsPath = "/models";
    internal const string PostChatCompletionPath = "/chat/completions";
    internal const string PostResponsesPath = "/responses";
    internal const string PostResponsesTokenCountPath = "/responses/input_tokens";
    internal const string PostEmbeddingPath = "/embeddings";
}