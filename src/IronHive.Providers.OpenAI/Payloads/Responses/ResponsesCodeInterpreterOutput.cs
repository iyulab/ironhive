using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterLogOutput), "log")]
[JsonDerivedType(typeof(ResponsesCodeInterpreterImageOutput), "image")]
public abstract class ResponsesCodeInterpreterOutput
{ }

public class ResponsesCodeInterpreterLogOutput : ResponsesCodeInterpreterOutput
{
    [JsonPropertyName("logs")]
    public required string Logs { get; set; }
}

public class ResponsesCodeInterpreterImageOutput : ResponsesCodeInterpreterOutput
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
