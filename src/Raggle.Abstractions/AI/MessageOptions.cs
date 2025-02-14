namespace Raggle.Abstractions.AI;

public enum MessageStrategy
{
    None,        // no reduction
    Truncate,    // truncate the messages
    Summarize,   // summarize the messages
}

public class MessageOptions
{
    /// <summary>
    /// the instructions to the model.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// the condensation strategy to use.
    /// if token limit is reached, the strategy will be applied.
    /// </summary>
    public MessageStrategy Strategy { get; set; }
}
