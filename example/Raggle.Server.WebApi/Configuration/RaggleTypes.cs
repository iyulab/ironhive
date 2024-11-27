using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AIServiceKeys
{
    OpenAI,
    Anthropic,
    Ollama,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HandlerServiceKeys
{
    Decoding,
    Chunking,
    Summarization,
    GenerateQA,
    Embeddings,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VectorStorageTypes
{
    LiteDB,
    Qdrant,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentStorageTypes
{
    LocalDisk,
    AzureBlob,
}
