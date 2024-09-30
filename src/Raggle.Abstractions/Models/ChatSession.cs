namespace Raggle.Abstractions.Models;

public class ChatSession
{
    public string? System { get; set; }

    public ChatHistory History { get; set; } = new ChatHistory();

    public string[] ToolKits { get; set; } = [];

    public int MaxTokens { get; set; } = 2048;

    public double? Temperature { get; set; }

    public int? TopK { get; set; }

    public double? TopP { get; set; }

    public string[]? StopSequences { get; set; }
}
