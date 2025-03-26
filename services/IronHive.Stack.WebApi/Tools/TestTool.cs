using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using System.ComponentModel;
using System.Diagnostics;
using Tavily;

namespace IronHive.Stack.WebApi.Tools;

public class TestTool : ToolHandlerBase
{
    private readonly HttpContext _context;

    public TestTool(IHttpContextAccessor accessor)
    {
        _context = accessor.HttpContext;
    }

    [FunctionTool(Name = "utc")]
    [Description("Get the current UTC time")]
    public DateTime Now()
    {
        return DateTime.UtcNow;
    }

    [FunctionTool("web_search")]
    [Description("perform a web search and retrieve search results")]
    public async Task<SearchResponse> SearchWebAsycn(
        [Description("search query")] string query)
    {
        using var client = new TavilyClient();
        var result = await client.SearchAsync(
            apiKey: "",
            query: query);
        return result;
    }

    [FunctionTool("window_cmd")]
    [Description("execute Windows command-line commands safely")]
    public async Task<string> ExecuteCommandAsync(
        [Description("command to execute in Windows Command Prompt")] string command)
    {
        // 위험한 명령어 차단
        //string[] blockedCommands = { "rm -rf", "format", "del", "shutdown" };
        //if (blockedCommands.Any(blockedCmd => command.Contains(blockedCmd,
        //    StringComparison.OrdinalIgnoreCase)))
        //{
        //    throw new InvalidOperationException("Potentially dangerous command blocked.");
        //}

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return string.IsNullOrWhiteSpace(error) ? output : throw new Exception(error);
    }

    public override Task<string> HandleSetInstructionsAsync(object? options)
    {
        if (options.TryConvertTo<TestToolOptions>(out var option))
        {
            return Task.FromResult(option.Description);
        }
        return base.HandleSetInstructionsAsync(options);
    }
}

public class TestToolOptions
{
    public string Description { get; set; }
}

