using System.ClientModel.Primitives;
using System.Text.Json;
using IronHive.Abstractions.Models;
using OpenAI.Models;

namespace IronHive.Providers.OpenAI;

public class OpenAIModelFinder : IModelFinder
{
    private readonly OpenAIModelClient _client;

    public OpenAIModelFinder(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIModelFinder(OpenAIConfig config)
    {
        _client = OpenAIClientFactory.Create(config).GetOpenAIModelClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<IModelCard>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        // Read raw: the SDK's model type drops the limits a gateway in front of OpenAI reports (OpenAIModelCards).
        var result = await _client.GetModelsAsync(new RequestOptions { CancellationToken = cancellationToken }).ConfigureAwait(false);
        using var document = JsonDocument.Parse(result.GetRawResponse().Content);
        return OpenAIModelCards.FromList(document.RootElement);
    }

    /// <inheritdoc />
    public virtual async Task<IModelCard?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.GetModelAsync(modelId, new RequestOptions { CancellationToken = cancellationToken }).ConfigureAwait(false);
            using var document = JsonDocument.Parse(result.GetRawResponse().Content);
            return OpenAIModelCards.FromEntry(document.RootElement);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }
}
