using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(SystemMessage), "system")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
[JsonDerivedType(typeof(ToolMessage), "tool")]
internal class Message { }

internal class SystemMessage : Message
{
    [JsonPropertyName("name")]
    internal string? Name { get; set; }

    [JsonPropertyName("content")]
    internal required string Content { get; set; }
}

internal class UserMessage : Message
{
    [JsonPropertyName("name")]
    internal string? Name { get; set; }

    /// <summary>
    /// Sets the content of the message. Use <see cref="MessageContent"/> Collection if attaching an image,
    /// otherwise use <see cref="string"/>.
    /// </summary>
    [JsonPropertyName("content")]
    internal required object Content { get; set; }
}

internal class AssistantMessage : Message
{
    [JsonPropertyName("name")]
    internal string? Name { get; set; }

    /// <summary>
    /// The content is necessary when ToolCalls is null.
    /// </summary>
    [JsonPropertyName("content")]
    internal string? Content { get; set; }

    [JsonPropertyName("refusal")]
    internal string? Refusal { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    internal ToolCall[]? ToolCalls { get; set; }
}

/// <summary>
/// This message is result of a tool call.
/// </summary>
internal class ToolMessage : Message
{
    [JsonPropertyName("tool_call_id")]
    internal required string ID { get; set; }

    [JsonPropertyName("content")]
    internal required string Content { get; set; }
}
