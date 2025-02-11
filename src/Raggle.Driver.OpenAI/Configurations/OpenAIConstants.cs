namespace Raggle.Driver.OpenAI.Configurations;

/// <summary>
/// <see href="https://platform.openai.com/docs/api-reference/introduction">OpenAI API Documentation</see>
/// </summary>
internal class OpenAIConstants
{
    internal const string DefaultEndPoint = "https://api.openai.com/";
    internal const string ApiKeyHeaderName = "Authorization";
    internal const string ApiKeyHeaderValue = $"Bearer {{0}}";
    internal const string OrganizationHeaderName = "OpenAI-Organization";
    internal const string ProjectHeaderName = "OpenAI-Project";

    internal const string GetModelListPath = "/v1/models";
    internal const string PostChatCompletionPath = "/v1/chat/completions";
    internal const string PostEmbeddingPath = "/v1/embeddings";
}
