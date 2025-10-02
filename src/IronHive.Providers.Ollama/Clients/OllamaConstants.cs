﻿namespace IronHive.Providers.Ollama.Clients;

/// <summary>
/// <see href="https://docs.ollama.com/api">Document</see>
/// </summary>
internal class OllamaConstants
{
    internal const string DefaultBaseUrl = "http://localhost:11434/api/";

    internal const string GetListModelsPath = "/tags";
    internal const string PostModelInfoPath = "/show";
    internal const string PostChatCompletionPath = "/chat";
    internal const string PostEmbeddingPath = "/embed";
}