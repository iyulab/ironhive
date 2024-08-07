using Spectre.Console;
using Raggle.Abstractions;

namespace Raggle.Console.UI;

public class ChatUI
{
    private readonly IRaggleService _raggle;

    public ChatUI(IRaggleService raggleService)
    {
        _raggle = raggleService;
    }

    public async Task StartAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold {Constants.BOT_COLOR}]{Constants.BOT_NAME} >[/] {Constants.WELCOME_MESSAGE}");
        while (true)
        {
            var prompt = AnsiConsole.Ask<string>($"[bold {Constants.USER_COLOR}]{Constants.USER_NAME} >[/] ").Trim();
            if (IsEqual(prompt, Constants.EXIT_COMMAND))
            {
                Environment.Exit(0);
                break;
            }
            if (IsEqual(prompt, Constants.CLEAR_COMMAND))
            {
                _raggle.ClearHistory();
                AnsiConsole.Clear();
                continue;
            }

            AnsiConsole.Markup($"[bold {Constants.BOT_COLOR}]{Constants.BOT_NAME} >[/] ");
            await foreach (var stream in _raggle.AskStreamingAsync(prompt))
            {
                AnsiConsole.Write(stream ?? "");
            }
            AnsiConsole.WriteLine();
        }
    }

    private static bool IsEqual(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
