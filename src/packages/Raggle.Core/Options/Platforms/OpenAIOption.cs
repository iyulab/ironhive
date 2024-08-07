using Raggle.Abstractions.Attributes;

namespace Raggle.Core.Options.Platforms;

public enum OpenAITextModel
{
    [EnumString("gpt-3.5-turbo")]
    GPT_3_5_Turbo,
    [EnumString("gpt-4")]
    GPT_4,
    [EnumString("gpt-4-turbo")]
    GPT_4_Turbo,
    [EnumString("gpt-4o")]
    GPT_4o,
}

public enum OpenAIEmbeddingModel
{
    [EnumString("text-embedding-3-large")]
    Text_Embedding_3_Large,
    [EnumString("text-embedding-3-small")]
    Text_Embedding_3_Small,
    [EnumString("text-embedding-ada-002")]
    Text_Embedding_Ada_002,
}

public class OpenAIOption
{
    public string ApiKey { get; set; } = string.Empty;
    public OpenAITextModel TextModel { get; set; } = OpenAITextModel.GPT_4o;
    public OpenAIEmbeddingModel EmbeddingModel { get; set; } = OpenAIEmbeddingModel.Text_Embedding_3_Large;
    public int TextModelMaxToken { get; set; } = 16_384;
    public int EmbeddingModelMaxToken { get; set; } = 8_191;
    public int MaxEmbeddingBatchSize { get; set; } = 100;
    public int MaxRetries { get; set; } = 10;
}
