using Anthropic;
using Anthropic.Models.Models;
using IronHive.Abstractions.Models;

namespace IronHive.Providers.Anthropic;

/// <inheritdoc />
public class AnthropicModelFinder : IModelFinder
{
    private readonly IAnthropicClient _client;
    private readonly IReadOnlyDictionary<string, AnthropicModelCapabilities>? _capabilityOverrides;

    public AnthropicModelFinder(string apiKey)
        : this(new AnthropicConfig { ApiKey = apiKey })
    { }

    public AnthropicModelFinder(AnthropicConfig config)
    {
        _client = AnthropicClientFactory.Create(config);
        _capabilityOverrides = config.ModelCapabilities is { Count: > 0 } overrides
            ? new Dictionary<string, AnthropicModelCapabilities>(overrides, StringComparer.Ordinal)
            : null;
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
        var models = await ListModelInfosAsync(cancellationToken);
        return models.Select(model =>
            ToCard(model.ID, model.DisplayName, model.CreatedAt, model.MaxInputTokens, model.MaxTokens, model.Capabilities)).ToList();
    }

    /// <summary>
    /// The policy each model gets on the wire, from what the vendor reports for it (<c>capabilities.thinking.types</c> and
    /// <c>capabilities.effort</c>) instead of the built-in table: keyed by model id, ready to assign to
    /// <see cref="AnthropicConfig.ModelCapabilities"/> (consumer entries win over the built-in table), so a model the table does
    /// not know yet is turned off, effort-capped and style-selected the way the model accepts. What the Models API does not
    /// state (a forced <c>tool_choice</c>) and every model whose answer carries no <c>capabilities</c> keep the value of the
    /// built-in row. See <see cref="FindCapabilitiesAsync"/> for the derivation. Nothing here is automatic: the generator never
    /// asks the API — the consumer calls this once and passes the result on.
    /// </summary>
    public async Task<IDictionary<string, AnthropicModelCapabilities>> ListCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await ListModelInfosAsync(cancellationToken);
        var policies = new Dictionary<string, AnthropicModelCapabilities>(StringComparer.Ordinal);
        foreach (var model in models)
            policies[model.ID] = DeriveCapabilities(model.ID, model.Capabilities);
        return policies;
    }

    /// <summary>
    /// The policy for one model from what the vendor reports for it, or <see langword="null"/> when the API does not know the
    /// model (like <see cref="FindModelAsync"/>). The Models API states per model which thinking types it accepts
    /// (<c>adaptive</c>, <c>enabled</c>, <c>disabled</c>, <c>between_tools</c>) and which effort levels: <see cref="AnthropicModelCapabilities.ThinkingStyle"/>
    /// is <see cref="AnthropicThinkingStyle.Adaptive"/> when <c>adaptive</c> is supported and <see cref="AnthropicThinkingStyle.Budget"/>
    /// when only <c>enabled</c> is; <see cref="AnthropicModelCapabilities.ThinkingOffStyle"/> is
    /// <see cref="AnthropicThinkingOffStyle.BetweenTools"/> when <c>between_tools</c> is supported, else
    /// <see cref="AnthropicThinkingOffStyle.Disabled"/> when <c>disabled</c> is, else <see cref="AnthropicThinkingOffStyle.LowEffort"/>;
    /// <see cref="AnthropicModelCapabilities.SupportsXHighEffort"/> is the <c>xhigh</c> effort flag. The answer is the wire truth —
    /// a model the vendor steers to a low effort instead of <c>disabled</c> (Claude Opus 5) still reports <c>disabled</c> as supported,
    /// and this method follows the report. A forced <c>tool_choice</c> is not part of <c>capabilities</c> and stays the built-in row's
    /// (or a consumer override's) value, as does everything for an answer that carries no <c>capabilities</c> (a gateway).
    /// </summary>
    public async Task<AnthropicModelCapabilities?> FindCapabilitiesAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _client.Models.Retrieve(new ModelRetrieveParams
            {
                ModelID = modelId
            }, cancellationToken);

            return DeriveCapabilities(model.ID, model.Capabilities);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private AnthropicModelCapabilities DeriveCapabilities(string modelId, ModelCapabilities? reported) =>
        AnthropicModelCapabilities.FromReported(reported, AnthropicModelCapabilities.Resolve(modelId, _capabilityOverrides));

    private async Task<List<ModelInfo>> ListModelInfosAsync(CancellationToken cancellationToken)
    {
        try
        {
            var page = await _client.Models.List(new ModelListParams
            {
                Limit = 1000,
            }, cancellationToken);

            var models = new List<ModelInfo>();
            await foreach (var model in page.Paginate(cancellationToken))
            {
                models.Add(model);
            }

            return models;
        }
        catch (Exception ex) when (AnthropicExceptionMapper.Map(ex, cancellationToken) is { } mapped)
        {
            // The same failure a message call with this key would raise (an invalid key is a ProviderHttpException 401).
            throw mapped;
        }
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
            
            return ToCard(model.ID, model.DisplayName, model.CreatedAt, model.MaxInputTokens, model.MaxTokens, model.Capabilities);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <summary>
    /// A card for one model: a <see cref="LanguageModelCard"/> when the API reports the model's limits
    /// (<c>max_input_tokens</c> → <see cref="LanguageModelCard.ContextWindow"/>, <c>max_tokens</c> →
    /// <see cref="LanguageModelCard.MaxOutputTokens"/>) or its <c>capabilities</c> (→
    /// <see cref="LanguageModelCard.Features"/> and <see cref="LanguageModelCard.InputModalities"/>, see
    /// <see cref="Describe"/>), a plain <see cref="ModelCard"/> otherwise.
    /// </summary>
    internal static IModelCard ToCard(
        string id,
        string displayName,
        DateTimeOffset createdAt,
        long? maxInputTokens,
        long? maxTokens,
        ModelCapabilities? capabilities = null)
    {
        if (maxInputTokens is not > 0 && maxTokens is not > 0 && capabilities is null)
        {
            return new ModelCard
            {
                ModelId = id,
                DisplayName = displayName,
                CreatedAt = createdAt.UtcDateTime,
            };
        }

        var (features, inputModalities) = Describe(capabilities);
        return new LanguageModelCard
        {
            ModelId = id,
            DisplayName = displayName,
            CreatedAt = createdAt.UtcDateTime,
            ContextWindow = maxInputTokens is > 0 ? (int)Math.Min(maxInputTokens.Value, int.MaxValue) : null,
            MaxOutputTokens = maxTokens is > 0 ? (int)Math.Min(maxTokens.Value, int.MaxValue) : null,
            Features = features,
            InputModalities = inputModalities,
        };
    }

    /// <summary>
    /// What the Models API says the model accepts, in the card's own vocabulary. Only a capability the vendor reports as
    /// supported is listed — an absent entry means «not reported as supported», and both lists stay <see langword="null"/>
    /// when the answer carries no <c>capabilities</c> at all (a gateway that echoes only the id).
    /// <list type="bullet">
    /// <item><description><see cref="LanguageModelCard.InputModalities"/>: <c>text</c> (every language model),
    /// <c>image</c> (<c>image_input</c>), <c>pdf</c> (<c>pdf_input</c>).</description></item>
    /// <item><description><see cref="LanguageModelCard.Features"/>: <c>thinking</c>, <c>effort</c>,
    /// <c>structured-output</c>, <c>citations</c>, <c>batch</c>, <c>web-search</c>, <c>code-execution</c>.</description></item>
    /// </list>
    /// The thinking types and effort levels behind <c>thinking</c> / <c>effort</c> are not carried here.
    /// </summary>
    internal static (IReadOnlyCollection<string>? Features, IReadOnlyCollection<string>? InputModalities) Describe(
        ModelCapabilities? capabilities)
    {
        if (capabilities is null)
            return (null, null);

        var input = new List<string> { "text" };
        if (capabilities.ImageInput is { Supported: true }) input.Add("image");
        if (capabilities.PdfInput is { Supported: true }) input.Add("pdf");

        var features = new List<string>();
        if (capabilities.Thinking is { Supported: true }) features.Add("thinking");
        if (capabilities.Effort is { Supported: true }) features.Add("effort");
        if (capabilities.StructuredOutputs is { Supported: true }) features.Add("structured-output");
        if (capabilities.Citations is { Supported: true }) features.Add("citations");
        if (capabilities.Batch is { Supported: true }) features.Add("batch");
        if (capabilities.ServerTools?.WebSearch is { Supported: true }) features.Add("web-search");
        if (capabilities.ServerTools?.CodeExecution is { Supported: true }) features.Add("code-execution");

        return (features, input);
    }
}
