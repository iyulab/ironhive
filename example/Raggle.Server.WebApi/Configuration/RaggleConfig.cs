using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.AzureBlob;
using Raggle.Driver.LiteDB;
using Raggle.Driver.LocalDisk;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Driver.Qdrant;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi.Configuration;

public class RaggleConfig
{
    [JsonIgnore]
    public static JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    public string ConnectionString { get; set; } = string.Empty;

    public AIServiceConfig AIService { get; set; } = new AIServiceConfig();

    public VectorStorageConfig VectorStorage { get; set; } = new VectorStorageConfig();

    public DocumentStorageConfig DocumentStorage { get; set; } = new DocumentStorageConfig();

    #region Inner Classes

    public class AIServiceConfig
    {
        public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();
        public AnthropicConfig Anthropic { get; set; } = new AnthropicConfig();
        public OllamaConfig Ollama { get; set; } = new OllamaConfig();
    }

    public class VectorStorageConfig
    {
        public VectorStorageTypes Type { get; set; } = default;
        public LiteDBConfig LiteDB { get; set; } = new LiteDBConfig();
        public QdrantConfig Qdrant { get; set; } = new QdrantConfig();
    }

    public class DocumentStorageConfig
    {
        public DocumentStorageTypes Type { get; set; } = default;
        public LocalDiskConfig LocalDisk { get; set; } = new LocalDiskConfig();
        public AzureBlobConfig AzureBlob { get; set; } = new AzureBlobConfig();
    }

    #endregion
}
