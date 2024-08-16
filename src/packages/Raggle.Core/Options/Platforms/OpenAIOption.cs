using Raggle.Abstractions.Attributes;

namespace Raggle.Core.Options.Platforms;

public class OpenAIOption
{
    public string ApiKey { get; set; } = string.Empty;
    public string TextModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
    public int TextModelMaxToken { get; set; } = 16_384;
    public int EmbeddingModelMaxToken { get; set; } = 8_191;
    public int MaxEmbeddingBatchSize { get; set; } = 100;
    public int MaxRetries { get; set; } = 10;
}
