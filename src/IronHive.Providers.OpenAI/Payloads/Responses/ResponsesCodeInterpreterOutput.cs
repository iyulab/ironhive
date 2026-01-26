using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterLogOutput), "log")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterImageOutput), "image")]
internal abstract class ResponsesCodeInterpreterOutput
{ }

internal class ResponsesCodeInterpreterLogOutput : ResponsesCodeInterpreterOutput
{
    [JsonPropertyName("logs")]
    public required string Logs { get; set; }
}

internal class ResponsesCodeInterpreterImageOutput : ResponsesCodeInterpreterOutput
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
