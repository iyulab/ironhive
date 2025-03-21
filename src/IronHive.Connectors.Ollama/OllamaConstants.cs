namespace IronHive.Connectors.Ollama;

internal class OllamaConstants
{
    internal const string DefaultBaseUrl = "http://localhost:11434/api/";

    internal const string GetModelListPath = "/tags";
    internal const string PostChatCompletionPath = "/chat";
    internal const string PostEmbeddingPath = "/embed";
}
