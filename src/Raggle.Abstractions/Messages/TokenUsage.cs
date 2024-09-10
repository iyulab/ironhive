namespace Raggle.Abstractions.Messages;

public class TokenUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }

    public int TotalTokens
    {
        get
        {
            return InputTokens + OutputTokens;
        }
    }
}
