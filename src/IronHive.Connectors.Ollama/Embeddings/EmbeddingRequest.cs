﻿using IronHive.Connectors.Ollama.Clients;
using System.Text.Json.Serialization;

namespace IronHive.Connectors.Ollama.Embeddings;

internal class EmbeddingRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("input")]
    public required IEnumerable<string> Input { get; set; }

    [JsonPropertyName("options")]
    public OllamaModelOptions? Options { get; set; }

    [JsonPropertyName("truncate")]
    public bool Truncate { get; set; } = true;

    [JsonPropertyName("keep_alive")]
    public string KeepAlive { get; set; } = "5m";
}
