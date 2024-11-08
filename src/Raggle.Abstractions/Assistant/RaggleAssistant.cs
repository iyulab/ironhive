using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Abstractions.Assistant;

public class RaggleAssistant
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Instructions { get; set; }

    public FunctionTool[]? Tools { get; set; }

    public ExecuteSettings? ExecuteSettings { get; set; }

    public IChatCompletionService ChatCompletionService { get; set; }


}

public class ExecuteSettings
{
    public required string Model { get; set; }

    public int? MaxTokens { get; set; }

    public float? Temperature { get; set; }

    public int? TopK { get; set; }

    public float? TopP { get; set; }

    public string[]? StopSequences { get; set; }
}
