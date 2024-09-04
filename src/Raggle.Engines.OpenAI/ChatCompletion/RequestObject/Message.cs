using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(SystemMessage), "system")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
[JsonDerivedType(typeof(ToolMessage), "tool")]
public class Message { }

public class SystemMessage : Message
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class UserMessage : Message
{
    [JsonPropertyName("content")]
    public required MessageContent Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class AssistantMessage : Message
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_calls")]
    public ICollection<object>? ToolCalls { get; set; }
}

public class ToolMessage : Message
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("tool_call_id")]
    public required string ToolCallID { get; set; }
}

