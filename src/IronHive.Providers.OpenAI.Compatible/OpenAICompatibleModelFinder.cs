using System.ClientModel.Primitives;
using System.Text.Json;
using IronHive.Abstractions.Models;
using IronHive.Providers.OpenAI;
using OpenAI.Models;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Lists the models of an OpenAI-compatible server and keeps the context length a self-hosted server reports with
/// each one.
/// </summary>
/// <remarks>
/// <para>
/// The OpenAI SDK's model type drops every field the OpenAI API does not define, so the response is read raw. An entry
/// that reports a context window or a maximum output (vLLM <c>max_model_len</c>, OpenRouter <c>context_length</c> and
/// <c>top_provider.max_completion_tokens</c>, Groq <c>context_window</c> — the full list is on
/// <see cref="OpenAIModelCards"/>) is a <see cref="LanguageModelCard"/>; any other entry is a plain
/// <see cref="ModelCard"/>. Servers that report nothing (Ollama, GPUStack's <c>meta: null</c>) give plain cards.
/// </para>
/// </remarks>
public class OpenAICompatibleModelFinder : IModelFinder
{
    private readonly OpenAIModelClient _client;

    /// <summary>Creates a finder for the server <paramref name="config"/> describes.</summary>
    public OpenAICompatibleModelFinder(OpenAICompatibleConfig config)
        : this((config ?? throw new ArgumentNullException(nameof(config))).ToOpenAI())
    { }

    /// <summary>Creates a finder from the OpenAI wire configuration of an OpenAI-compatible server.</summary>
    public OpenAICompatibleModelFinder(OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _client = OpenAIClientFactory.Create(config).GetOpenAIModelClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<IModelCard>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _client.GetModelsAsync(new RequestOptions { CancellationToken = cancellationToken }).ConfigureAwait(false);
        using var document = JsonDocument.Parse(result.GetRawResponse().Content);
        return OpenAIModelCards.FromList(document.RootElement);
    }

    /// <summary>
    /// Finds a model in the server's list — the same card <see cref="ListModelsAsync"/> returns for it.
    /// </summary>
    /// <remarks>
    /// The list is the one route every OpenAI-compatible server serves: vLLM and llama.cpp's server have no
    /// <c>/v1/models/{id}</c>, so asking for the model by id finds nothing on exactly the servers that report a context
    /// length. Ids are compared ordinally, as the servers do.
    /// </remarks>
    public virtual async Task<IModelCard?> FindModelAsync(string modelId, CancellationToken cancellationToken)
    {
        try
        {
            var models = await ListModelsAsync(cancellationToken).ConfigureAwait(false);
            return models.FirstOrDefault(model => string.Equals(model.ModelId, modelId, StringComparison.Ordinal));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Same contract as the OpenAI finder this replaces: a model that cannot be found, or a server that cannot
            // be asked, is null to the caller (ModelService passes it through). Cancellation still propagates.
            return null;
        }
    }

    /// <summary>One list entry as a card, or null when it has no id — <see cref="OpenAIModelCards"/>'s reading.</summary>
    internal static IModelCard? ToModelCard(JsonElement entry) => OpenAIModelCards.FromEntry(entry);
}
