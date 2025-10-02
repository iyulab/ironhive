﻿using IronHive.Providers.OpenAI.Payloads.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIModelsClient : OpenAIClientBase
{
    public OpenAIModelsClient(OpenAIConfig config) : base(config)
    { }

    public OpenAIModelsClient(string baseUrl) : base(baseUrl)
    { }

    public async Task<IEnumerable<OpenAIModel>> GetListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var jsonDoc = await _client.GetFromJsonAsync<JsonDocument>(
            OpenAIConstants.GetModelsPath.RemovePrefix('/'), _jsonOptions, cancellationToken);

        var models = jsonDoc?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>(_jsonOptions);
        return models?.OrderByDescending(m => m.Created)
            .ToArray() ?? [];
    }

    public async Task<OpenAIModel?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(OpenAIConstants.GetModelsPath, modelId).RemovePrefix('/');
        var model = await _client.GetFromJsonAsync<OpenAIModel>(path, _jsonOptions, cancellationToken);
        return model;
    }
}