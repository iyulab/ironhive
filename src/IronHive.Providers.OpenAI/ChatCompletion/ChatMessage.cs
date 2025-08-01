﻿using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI.JsonConverters;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(DeveloperChatMessage), "developer")]
[JsonDerivedType(typeof(SystemChatMessage), "system")]
[JsonDerivedType(typeof(UserChatMessage), "user")]
[JsonDerivedType(typeof(AssistantChatMessage), "assistant")]
[JsonDerivedType(typeof(ToolChatMessage), "tool")]
public abstract class ChatMessage 
{ }

public class DeveloperChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class SystemChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class UserChatMessage : ChatMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("content")]
    [JsonConverter(typeof(ChatMessageContentJsonConverter))]
    public ICollection<ChatMessageContent> Content { get; set; } = new List<ChatMessageContent>();
}

public class AssistantChatMessage : ChatMessage
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
    /// when using web search tools, urls are returned.
    /// </summary>
    [JsonPropertyName("annotations")]
    public object? Annotations { get; set; }

    /// <summary>
    /// If the audio output modality is requested
    /// </summary>
    [JsonPropertyName("audio")]
    public object? Audio { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<OpenAIFunctionToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// This message is result of a tool call.
/// </summary>
public class ToolChatMessage : ChatMessage
{
    [JsonPropertyName("tool_call_id")]
    public required string ID { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }
}