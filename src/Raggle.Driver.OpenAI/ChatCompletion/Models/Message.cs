using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(DeveloperMessage), "developer")]
[JsonDerivedType(typeof(SystemMessage), "system")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
[JsonDerivedType(typeof(ToolMessage), "tool")]
internal class Message { }

internal class DeveloperMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

internal class SystemMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

internal class UserMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Sets the content of the message. Use <see cref="MessageContent"/> Collection if attaching an image,
    /// otherwise use <see cref="string"/>.
    /// </summary>
    [JsonPropertyName("content")]
    public required object Content { get; set; }
}

internal class AssistantMessage : Message
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The content is necessary when ToolCalls is null.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ToolCall[]? ToolCalls { get; set; }

    /// <summary>
    /// If the audio output modality is requested
    /// </summary>
    [JsonPropertyName("audio")]
    public object? Audio { get; set; }
}

/// <summary>
/// This message is result of a tool call.
/// </summary>
internal class ToolMessage : Message
{
    [JsonPropertyName("tool_call_id")]
    public required string ID { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}
