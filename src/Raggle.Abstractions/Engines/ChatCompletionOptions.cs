namespace Raggle.Abstractions.Engines;

public class ChatCompletionOptions
{
    public int MaxTokens { get; set; }

    public string? ToolChoice { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    public double? TopP { get; set; }

    public ICollection<string>? StopSequences { get; set; }
}
