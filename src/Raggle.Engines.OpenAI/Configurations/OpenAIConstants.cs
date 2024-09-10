namespace Raggle.Engines.OpenAI.Configurations;

/// <summary>
/// <see href="https://platform.openai.com/docs/api-reference/introduction">OpenAI API Documentation</see>
/// </summary>
internal class OpenAIConstants
{
    internal const string Host = "api.openai.com";
    internal const string AuthHeaderName = "Authorization";
    internal const string AuthHeaderValue = $"Bearer {{0}}";
    internal const string OrgHeaderName = "OpenAI-Organization";
    internal const string ProjectHeaderName = "OpenAI-Project";

    internal const string GetModelsPath = "/v1/models";
    internal const string PostChatCompletionPath = "/v1/chat/completions";
    internal const string PostEmbeddingPath = "/v1/embeddings";

}
