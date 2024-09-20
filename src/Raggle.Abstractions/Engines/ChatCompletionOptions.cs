using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.Engines;

public class ChatCompletionOptions
{
    public required string ModelId { get; set; }

    public int MaxTokens { get; set; } = 2048;

    public string? System { get; set; }

    public FunctionTool[]? Tools { get; set; }

    //public string? ToolChoice { get; set; }

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

    public string[]? StopSequences { get; set; }
}
