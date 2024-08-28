namespace Raggle.Server.Web.Options;

public class OpenAIOptions
{
    public string ChatModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
