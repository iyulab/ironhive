using dotenv.net;
using HtmlAgilityPack;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Core.Utilities;
using ModelContextProtocol.Protocol.Types;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Tavily;

namespace WebServer.Tools;

public class TestTool
{
    //private readonly HttpContext _context;

    //public TestTool(IHttpContextAccessor accessor)
    //{
    //    _context = accessor.HttpContext
    //        ?? throw new InvalidOperationException("HttpContext is not available.");
    //}

    [FunctionTool(Name = "utc")]
    [Description("Get the current UTC time")]
    public DateTime Now()
    {
        return DateTime.UtcNow;
    }

    [FunctionTool("get_web_content")]
    [Description("get web content from url")]
    public async Task<string> GetWebContentAsync(
        [Description("url for content")] string url, 
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync(url, cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var text = doc.DocumentNode.InnerText;
        return TextCleaner.Clean(text);
    }

    [FunctionTool("web_search")]
    [Description("perform a web search and retrieve search results")]
    public async Task<SearchResponse> SearchWebAsycn(
        [Description("search query")] string query,
        CancellationToken cancellationToken)
    {
        var env = DotEnv.Read();
        using var client = new TavilyClient();
        var result = await client.SearchAsync(
            apiKey: env.TryGetValue("TAVILY", out var value) ? value : string.Empty,
            query: query,
            cancellationToken: cancellationToken);
        return result;
    }

    [FunctionTool("window_cmd")]
    [Description("execute Windows command-line commands safely")]
    public async Task<string> ExecuteCommandAsync(
        [Description("command to execute in Windows Command Prompt")] string command,
        CancellationToken cancellationToken)
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
            },
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(error) ? output : throw new Exception(error);
    }

    [FunctionTool("write_file")]
    [Description("save text file to user computer, if dont know where save path, question to user")]
    public async Task WriteFIleAsync(
        [Description("full absolute file path to save user computer contains file extensions")] string filePath,
        [Description("text content of file")] string text)
    {
        await File.WriteAllTextAsync(filePath, text);
    }
}

