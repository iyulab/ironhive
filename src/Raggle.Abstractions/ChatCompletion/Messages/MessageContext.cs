namespace Raggle.Abstractions.ChatCompletion.Messages;

public enum ReductionStrategy
{
    None,
    Summarize,
    Truncate,
}

public class MessageContext
{
    public ReductionStrategy ReductionStrategy { get; set; } = ReductionStrategy.None;

    public MessageCollection Messages { get; set; } = new();

    public int MaxLoopCount { get; set; } = 3;

    public int TokenUsageCount { get; set; } = 0;
}
