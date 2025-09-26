﻿namespace IronHive.Providers.OpenAI.Share;

/// <summary>
/// <see href="https://platform.openai.com/docs/api-reference/introduction">OpenAI API Documentation</see>
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
    internal const string PostEmbeddingPath = "/embeddings";
}
