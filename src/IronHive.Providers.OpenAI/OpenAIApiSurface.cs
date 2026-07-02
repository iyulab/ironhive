namespace IronHive.Providers.OpenAI;

/// <summary>
/// Selects which OpenAI HTTP API surface the message generator targets.
/// <para>
/// The two surfaces are wire-incompatible. <see cref="Responses"/> posts to <c>/v1/responses</c>
/// (OpenAI-proprietary; supports reasoning summaries and encrypted reasoning content).
/// <see cref="ChatCompletions"/> posts to <c>/v1/chat/completions</c> (the de-facto standard that
/// OpenAI-compatible servers such as Ollama, LM Studio, vLLM, llama.cpp server, and GPUStack implement).
/// </para>
/// <para>
/// Pointing a Chat-Completions-only endpoint at the Responses surface (or vice versa) fails at the wire
/// with <c>404 Not Found</c> — it compiles and restores cleanly, so the mismatch only shows in production.
/// </para>
/// </summary>
public enum OpenAIApiSurface
{
    /// <summary>
    /// Chat Completions API (<c>POST /v1/chat/completions</c>). Maximal compatibility — the surface
    /// OpenAI-compatible / self-hosted servers implement. Default for compatible and GPUStack providers.
    /// </summary>
    ChatCompletions,

    /// <summary>
    /// Responses API (<c>POST /v1/responses</c>). OpenAI-proprietary; required for reasoning summaries and
    /// <c>reasoning.encrypted_content</c>. Default for first-party OpenAI.
    /// </summary>
    Responses,
}
