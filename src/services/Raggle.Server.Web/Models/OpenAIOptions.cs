namespace Raggle.Server.API.Models;

public class OpenAIOptions
{
    public string ChatModel { get; set; }
    public string EmbeddingModel { get; set; }
    public string ApiKey { get; set; }
}