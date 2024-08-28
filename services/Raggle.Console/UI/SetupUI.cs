using Raggle.Console.Settings;
using Raggle.Console.UI.Setup;
using Spectre.Console;

namespace Raggle.Console.UI;

public class SetupUI
{
    public AppSettings Setup(string baseDir)
    {
        AnsiConsole.Clear();
        var settings = new AppSettings { WorkingDirectory = baseDir };

        var platformType = new PlatformSetup().Setup();
        settings.Platforms.Type = platformType;
        AnsiConsole.Clear();

        if (platformType == PlatformTypes.OpenAI)
        {
            var openAI = new OpenAISetup().Setup();
            settings.Platforms.OpenAI = openAI;
            AnsiConsole.Clear();
        }

        var prompt = new PromptSetup().Setup();
        settings.Prompts.SimplePrompt = prompt;
        AnsiConsole.Clear();

        return settings;
    }

    public static void Exit()
    {
        AnsiConsole.MarkupLine("[green]Exiting the setup. Goodbye![/]");
        Environment.Exit(0);
    }
}
