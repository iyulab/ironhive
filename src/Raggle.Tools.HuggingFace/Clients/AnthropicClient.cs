using Raggle.Tools.ModelSearch.Models;

namespace Raggle.Tools.ModelSearch.Clients;

/// <summary>
/// a search client for interacting with Anthropic models.
/// </summary>
public class AnthropicClient
{
    private static readonly IEnumerable<AnthropicModel> PredefinedModels =
    [
        new() { ModelId = "claude-3-5-sonnet-20240620" },
        new() { ModelId = "claude-3-opus-20240229" },
        new() { ModelId = "claude-3-sonnet-20240229" },
        new() { ModelId = "claude-3-haiku-20240307" },
        new() { ModelId = "claude-2.1" },
        new() { ModelId = "claude-2.0" },
        new() { ModelId = "claude-instant-1.2" },
    ];

    /// <summary>
    /// Gets the predefined Anthropic models.
    /// This list of models was last updated on 2024-08-28.
    /// Since there is no API provided by Anthropic to dynamically retrieve the model list,
    /// it is important to manually update this method when new models are available or old models are deprecated.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="AnthropicModel"/> objects.</returns>
    public IEnumerable<AnthropicModel> GetModels()
    {
        return PredefinedModels;
    }
}
