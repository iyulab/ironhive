using System.Text.Json.Serialization;

namespace IronHive.Providers.Declarations;

/// <summary>
/// The request/response format a server speaks - one value per IronHive wire adapter. A declaration names the wire;
/// the factory picks the adapter that owns that format's quirks.
/// </summary>
/// <remarks>Serialised by name (<c>"OpenAIChatCompletions"</c>), so a stored declaration survives a reordering of this enum.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WireFormat
{
    /// <summary>OpenAI Chat Completions (<c>/chat/completions</c>) - Ollama, LM Studio, vLLM, llama.cpp, GPUStack and most gateways. <c>IronHive.Providers.OpenAI.Compatible</c>.</summary>
    OpenAIChatCompletions = 1,

    /// <summary>OpenAI Responses (<c>/responses</c>). <c>IronHive.Providers.OpenAI</c>.</summary>
    OpenAIResponses = 2,

    /// <summary>Anthropic Messages (<c>/v1/messages</c>). <c>IronHive.Providers.Anthropic</c>.</summary>
    Anthropic = 3,

    /// <summary>Google Gemini (<c>generateContent</c>). <c>IronHive.Providers.GoogleAI</c>.</summary>
    GoogleAI = 4,
}
