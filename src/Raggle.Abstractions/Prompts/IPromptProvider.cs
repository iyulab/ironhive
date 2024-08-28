namespace Raggle.Abstractions.Prompts;

public interface IPromptProvider
{
    string GetPrompt();
    string GetPromptWithInfo(string information);
}
