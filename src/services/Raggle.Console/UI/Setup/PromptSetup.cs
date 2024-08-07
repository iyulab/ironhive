using Raggle.Core.Options.Prompts;
using Spectre.Console;

namespace Raggle.Console.UI.Setup;

public class PromptSetup
{
    public SimplePromptOption Setup()
    {
        AnsiConsole.MarkupLine(
"""
Welcome to the file description step!
            
In this step, you will provide a description of the files to the bot. 
This information is crucial as it helps the bot understand the context 
and content of the files you are working with. A good description can 
significantly enhance the bot's ability to assist you effectively.

Here are some tips for a good file description:
1. **Be Specific**: Include key details about the file content, such as main topics, important sections, or special formats.
2. **Be Clear**: Use clear and concise language to avoid any confusion.
3. **Be Relevant**: Only include information that is directly related to the files' purpose and usage.

Please follow the prompt below to provide your file description.
""");

        var userPrompt = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter your description:"));

        return new SimplePromptOption
        {
            SystemPrompt = Constants.DEFAULT_SYSTEM_PROMPT,
            UserPrompt = userPrompt
        };
    }
}
