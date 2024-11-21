using System.Text.Json.Serialization;

namespace Raggle.Server.WebApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RaggleServiceKeys
{
    OpenAI,
    Anthropic,
    Ollama,
    LocalDisk,
    LiteDB,
}
