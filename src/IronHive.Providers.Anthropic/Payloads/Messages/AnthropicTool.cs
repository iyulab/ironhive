﻿using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

/// <summary>
/// Custom tool only, not use other tools
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CustomAnthropicTool), "custom")]
internal abstract class AnthropicTool
{ }

internal class CustomAnthropicTool : AnthropicTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public required object InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}