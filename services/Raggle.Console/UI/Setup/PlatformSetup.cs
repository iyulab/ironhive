using Raggle.Console.Settings;
using Spectre.Console;

namespace Raggle.Console.UI.Setup;

// 어떤 AI 플랫폼을 사용할지 설정하는 스텝
public class PlatformSetup
{
    public PlatformTypes Setup()
    {
        AnsiConsole.MarkupLine(
"""
Welcome to the AI Chat Application!
In this step, you will select the AI platform you want to use for generating responses. 
We currently support multiple platforms, each offering unique features and capabilities. 

1. **OpenAI**: Known for its powerful language models, OpenAI provides state-of-the-art text generation and understanding capabilities. If you are looking for high-quality and versatile AI-driven conversations, OpenAI is a great choice.

Please select the AI platform you want to use for this session:
""");

        var platform = AnsiConsole.Prompt(
            new SelectionPrompt<PlatformTypes>()
                .Title("Please select the AI platform:")
                .PageSize(3)
                .AddChoices(PlatformTypes.OpenAI));

        return platform;
    }
}
