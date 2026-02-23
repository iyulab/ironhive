using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// Distills verbose sub-agent results into compact summaries before
/// returning them to the parent orchestrator. Reduces context accumulation
/// in multi-agent scenarios where sub-agents produce lengthy outputs.
/// </summary>
public interface IResultDistiller
{
    /// <summary>
    /// Distills the agent's response into a concise summary.
    /// </summary>
    /// <param name="agentName">The name of the agent whose result is being distilled.</param>
    /// <param name="response">The original response from the agent.</param>
    /// <param name="options">Optional distillation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A distilled response with compact content.</returns>
    Task<MessageResponse> DistillAsync(
        string agentName,
        MessageResponse response,
        ResultDistillationOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for result distillation.
/// </summary>
public class ResultDistillationOptions
{
    /// <summary>
    /// Maximum character length of the distilled result.
    /// Results shorter than this are not distilled.
    /// Default: 2000.
    /// </summary>
    public int MaxOutputChars { get; init; } = 2000;

    /// <summary>
    /// Minimum character length of the original result to trigger distillation.
    /// Short results are passed through unchanged.
    /// Default: 3000.
    /// </summary>
    public int MinInputCharsForDistillation { get; init; } = 3000;

    /// <summary>
    /// Whether to preserve tool call information in the distilled result.
    /// Default: true.
    /// </summary>
    public bool PreserveToolCalls { get; init; } = true;
}
