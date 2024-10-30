namespace Raggle.Connector.OpenAI.Configurations;

/// <summary>
/// <see href="https://platform.openai.com/docs/api-reference/introduction">OpenAI API Documentation</see>
/// </summary>
internal class OpenAIConstants
{
    internal const string DefaultEndPoint = "https://api.openai.com/v1/";
    internal const string ApiKeyHeaderName = "Authorization";
    internal const string ApiKeyHeaderValue = $"Bearer {{0}}";
    internal const string OrganizationHeaderName = "OpenAI-Organization";
    internal const string ProjectHeaderName = "OpenAI-Project";

    internal const string GetModelsPath = "models";
    internal const string PostChatCompletionPath = "chat/completions";
    internal const string PostEmbeddingPath = "embeddings";

}
