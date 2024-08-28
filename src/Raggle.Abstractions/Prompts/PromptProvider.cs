namespace Raggle.Abstractions.Prompts;

public abstract class PromptProvider : IPromptProvider
{
    public abstract string GetPrompt();

    public string GetPromptWithInfo(string information)
    {
        var prompt = GetPrompt();
        var promptWithInfo = $"""
{prompt} 

[INFORMATION]
{information}
""";
        return promptWithInfo;
    }
}
