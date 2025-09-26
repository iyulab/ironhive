﻿using System.Text.Json;

namespace IronHive.Providers.OpenAI.Share;

public abstract class OpenAIClientBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected OpenAIClientBase(OpenAIConfig config)
    {
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected OpenAIClientBase(string apiKey)
    {
        var config = new OpenAIConfig { ApiKey = apiKey };
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="OpenAIClientBase"/> instance.
    /// </summary>
    public virtual void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpClient"/> with the specified configuration settings.
    /// </summary>
    private static HttpClient CreateHttpClient(OpenAIConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrWhiteSpace(config.BaseUrl)
                ? new Uri(OpenAIConstants.DefaultBaseUrl.EnsureSuffix('/'))
                : new Uri(config.BaseUrl.EnsureSuffix('/'))
        };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.AuthorizationHeaderName, string.Format(OpenAIConstants.AuthorizationHeaderValue, config.ApiKey));

        if (!string.IsNullOrWhiteSpace(config.Organization))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.OrganizationHeaderName, config.Organization);

        if (!string.IsNullOrWhiteSpace(config.Project))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.ProjectHeaderName, config.Project);

        return client;
    }
}
