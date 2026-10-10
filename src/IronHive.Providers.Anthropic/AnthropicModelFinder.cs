using Anthropic;
using Anthropic.Models.Models;
using IronHive.Abstractions.Models;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicModelFinder : IModelFinder
{
    private readonly IAnthropicClient _client;

    public AnthropicModelFinder(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicModelFinder(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelCard>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var page = await _client.Models.List(new ModelListParams
        {
            Limit = 1000,
        }, cancellationToken);

        var models = new List<IModelCard>();
        await foreach (var model in page.Paginate(cancellationToken))
        {
            models.Add(ToCard(model.ID, model.DisplayName, model.CreatedAt, model.MaxInputTokens, model.MaxTokens));
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<IModelCard?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _client.Models.Retrieve(new ModelRetrieveParams
            {
                ModelID = modelId
            }, cancellationToken);
            
            return ToCard(model.ID, model.DisplayName, model.CreatedAt, model.MaxInputTokens, model.MaxTokens);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <summary>
    /// A card for one model: a <see cref="LanguageModelCard"/> when the API reports the model's limits
    /// (<c>max_input_tokens</c> → <see cref="LanguageModelCard.ContextWindow"/>, <c>max_tokens</c> →
    /// <see cref="LanguageModelCard.MaxOutputTokens"/>), a plain <see cref="ModelCard"/> otherwise.
    /// </summary>
    internal static IModelCard ToCard(string id, string displayName, DateTimeOffset createdAt, long? maxInputTokens, long? maxTokens)
    {
        if (maxInputTokens is not > 0 && maxTokens is not > 0)
        {
            return new ModelCard
            {
                ModelId = id,
                DisplayName = displayName,
                CreatedAt = createdAt.UtcDateTime,
            };
        }

        return new LanguageModelCard
        {
            ModelId = id,
            DisplayName = displayName,
            CreatedAt = createdAt.UtcDateTime,
            ContextWindow = maxInputTokens is > 0 ? (int)Math.Min(maxInputTokens.Value, int.MaxValue) : null,
            MaxOutputTokens = maxTokens is > 0 ? (int)Math.Min(maxTokens.Value, int.MaxValue) : null,
        };
    }
}
