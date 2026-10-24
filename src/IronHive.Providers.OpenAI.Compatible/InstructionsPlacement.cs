namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Where a Chat Completions request puts its per-request instructions (<c>MessageGenerationRequest.Instructions</c>).
/// </summary>
/// <remarks>
/// <para>
/// Every placement keeps the system prompt as the leading text, and none stores the instructions in the conversation, so
/// a server's prefix cache reuses the prompt when only the instructions change. They differ in what else stays cached,
/// and in which chat templates accept them.
/// </para>
/// <para>
/// A template that renders the tool definitions inside the first system turn (Qwen 2.5/3, Llama 3.x) puts anything
/// appended to that turn ahead of the tools, so the tools are re-read whenever the instructions change.
/// </para>
/// </remarks>
public enum InstructionsPlacement
{
    /// <summary>
    /// Appended to the end of the system message (default). Every chat template accepts it; on a template that renders
    /// the tools inside the system turn, the tools are not cached across different instructions.
    /// </summary>
    SystemAppend = 0,

    /// <summary>
    /// Sent as a second <c>system</c> message after the first. Qwen 2.5/3 and Llama 3.x place it after the tools, so the
    /// tools stay cached; Gemma 3 rejects a second system message, the DeepSeek-R1 distills keep only the last one (the
    /// prompt is lost) and gpt-oss keeps only the first (the instructions are lost).
    /// </summary>
    SeparateSystemMessage = 1,

    /// <summary>
    /// Leading the content of the request's last <c>user</c> message, separated from it by a blank line. Every chat
    /// template accepts it, and the system prompt, the tools and the earlier turns stay cached. A request with no user
    /// message falls back to <see cref="SystemAppend"/>.
    /// </summary>
    LeadingUserTurn = 2,
}
