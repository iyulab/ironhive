﻿using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.Tokens;

internal class CountTokensRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("contents")]
    public ICollection<Content>? Contents { get; set; }
}