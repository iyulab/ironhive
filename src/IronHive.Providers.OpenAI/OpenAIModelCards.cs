using System.Text.Json;
using IronHive.Abstractions.Models;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Reads one entry of an OpenAI-wire model list (<c>GET /models</c> or <c>GET /models/{id}</c>) raw, keeping the limits a
/// server or gateway reports beside the fields the OpenAI API defines. The OpenAI SDK's model type drops every field the
/// OpenAI API does not define, so both OpenAI-wire finders read the response themselves and share this reading.
/// </summary>
/// <remarks>
/// <para>Context window, first one present: <c>max_model_len</c> (vLLM), <c>context_length</c> (OpenRouter, Together,
/// Fireworks), <c>context_window</c> (Groq), <c>max_context_length</c> (Mistral), <c>top_provider.context_length</c>
/// (OpenRouter).</para>
/// <para>Maximum output, first one present: <c>max_output_tokens</c>, <c>max_completion_tokens</c> (Groq),
/// <c>top_provider.max_completion_tokens</c> (OpenRouter).</para>
/// <para>A field whose value is not a positive whole number (<c>null</c>, a string, a fraction) counts as absent, so the
/// next name is tried; one odd entry never costs the rest of the list.</para>
/// <para>Input modalities, first one present: <c>architecture.input_modalities</c> (OpenRouter), else the input side of
/// <c>architecture.modality</c>. Null when neither is stated (vLLM and llama.cpp lists state nothing).</para>
/// <para>An entry with a limit or input modalities is a <see cref="LanguageModelCard"/>; any other is a plain <see cref="ModelCard"/>.
/// llama.cpp's <c>meta.n_ctx_train</c> is deliberately not read: it is the context the model was trained with, not the
/// context the server was started with, and a larger number than the server accepts is worse than none.</para>
/// </remarks>
internal static class OpenAIModelCards
{
    private static readonly string[] ContextFields = ["max_model_len", "context_length", "context_window", "max_context_length"];
    private static readonly string[] OutputFields = ["max_output_tokens", "max_completion_tokens"];
    private static readonly long MinUnixSeconds = DateTimeOffset.MinValue.ToUnixTimeSeconds();
    private static readonly long MaxUnixSeconds = DateTimeOffset.MaxValue.ToUnixTimeSeconds();

    /// <summary>One model object as a card, or null when it has no id.</summary>
    public static IModelCard? FromEntry(JsonElement entry)
    {
        if (entry.ValueKind != JsonValueKind.Object
            || !entry.TryGetProperty("id", out var idElement)
            || idElement.ValueKind != JsonValueKind.String
            || idElement.GetString() is not { Length: > 0 } id)
        {
            return null;
        }

        var ownedBy = entry.TryGetProperty("owned_by", out var owner) && owner.ValueKind == JsonValueKind.String
            ? owner.GetString()
            : null;
        DateTime? createdAt = entry.TryGetProperty("created", out var created)
            && created.ValueKind == JsonValueKind.Number
            && created.TryGetInt64(out var seconds)
            && seconds >= MinUnixSeconds && seconds <= MaxUnixSeconds
            ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
            : null;

        var topProvider = entry.TryGetProperty("top_provider", out var top) && top.ValueKind == JsonValueKind.Object
            ? top
            : (JsonElement?)null;
        var contextWindow = FirstPositive(entry, ContextFields) ?? FirstPositive(topProvider, ["context_length"]);
        var maxOutput = FirstPositive(entry, OutputFields) ?? FirstPositive(topProvider, ["max_completion_tokens"]);

        var inputModalities = ReadInputModalities(entry);

        if (contextWindow is null && maxOutput is null && inputModalities is null)
        {
            return new ModelCard
            {
                ModelId = id,
                DisplayName = id,
                OwnedBy = ownedBy,
                CreatedAt = createdAt,
            };
        }

        return new LanguageModelCard
        {
            ModelId = id,
            DisplayName = id,
            OwnedBy = ownedBy,
            CreatedAt = createdAt,
            ContextWindow = contextWindow,
            MaxOutputTokens = maxOutput,
            InputModalities = inputModalities,
        };
    }

    /// <summary>
    /// The input modalities an entry states, lower-cased and de-duplicated in the order given, or null when it states none:
    /// <c>architecture.input_modalities</c> (OpenRouter, an array of strings), else the input side of
    /// <c>architecture.modality</c> (<c>"text+image-&gt;text"</c> gives <c>text</c>, <c>image</c>). A value of another shape, an
    /// empty array and non-string elements count as absent, so the next source is tried and one odd entry costs nothing.
    /// </summary>
    private static string[]? ReadInputModalities(JsonElement entry)
    {
        if (!entry.TryGetProperty("architecture", out var architecture) || architecture.ValueKind != JsonValueKind.Object)
            return null;

        if (architecture.TryGetProperty("input_modalities", out var listed) && listed.ValueKind == JsonValueKind.Array)
        {
            var fromArray = Distinct(listed.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()));
            if (fromArray is not null)
                return fromArray;
        }

        if (architecture.TryGetProperty("modality", out var modality)
            && modality.ValueKind == JsonValueKind.String
            && modality.GetString() is { } text)
        {
            var arrow = text.IndexOf("->", StringComparison.Ordinal);
            var inputSide = arrow >= 0 ? text[..arrow] : text;
            return Distinct(inputSide.Split('+'));
        }

        return null;
    }

    private static string[]? Distinct(IEnumerable<string?> names)
    {
        var result = names
            .Select(n => n?.Trim().ToLowerInvariant())
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .Select(n => n!)
            .ToArray();
        return result.Length == 0 ? null : result;
    }

    /// <summary>Every entry of a list response's <c>data</c> array as cards, newest first.</summary>
    public static List<IModelCard> FromList(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return data.EnumerateArray()
            .Select(FromEntry)
            .OfType<IModelCard>()
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    private static int? FirstPositive(JsonElement? element, string[] names)
    {
        if (element is not { ValueKind: JsonValueKind.Object } obj)
            return null;

        // A value that is not a whole number (null - OpenRouter's router entries -, a string, a fraction) is skipped
        // like an absent field: TryGetInt32 throws on a non-number element instead of returning false.
        foreach (var name in names)
        {
            if (obj.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var number)
                && number > 0)
            {
                return number;
            }
        }
        return null;
    }
}
