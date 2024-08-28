using Raggle.Abstractions.Prompts;
using Raggle.Core.Options.Prompts;
using System.Text;

namespace Raggle.Core.Prompts;

public class SimplePromptProvider : PromptProvider
{
    private readonly string prompt;

    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;

    public SimplePromptProvider(SimplePromptOption promptOption)
    {
        SystemPrompt = promptOption.SystemPrompt;
        UserPrompt = promptOption.UserPrompt;
        prompt = BuildPrompt();
    }

    public override string GetPrompt()
    {
        return prompt;
    }

    private string BuildPrompt()
    {
        var promptBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(SystemPrompt))
        {
            promptBuilder.AppendLine(SystemPrompt);
        }

        if (!string.IsNullOrWhiteSpace(UserPrompt))
        {
            promptBuilder.AppendLine("[User Instructions]");
            promptBuilder.AppendLine(UserPrompt);
        }

        return promptBuilder.ToString();
    }
}
