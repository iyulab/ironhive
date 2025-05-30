using dotenv.net;
using HtmlAgilityPack;
using IronHive.Core.Tools;
using IronHive.Core.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using Tavily;

namespace WebServer.Tools;

public class TestTool
{
    [FunctionTool("extract_web")]
    [Description("Fetches and extracts text content from a web page.")]
    public async Task<object> GetWebContentAsync(
        [Description("The URL to fetch content from.")] string url, 
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        });
        var html = await client.GetStringAsync(url, cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var title = doc.DocumentNode.SelectSingleNode("/html/head/title")?.InnerText;
        var body = doc.DocumentNode.SelectSingleNode("/html/body");
        var content = TextCleaner.Clean(body?.InnerText ?? string.Empty);

        return new
        {
            Title = title,
            Content = content
        };
    }

    [FunctionTool(Name = "search_web")]
    [Description("Performs a web search and returns the results.")]
    public async Task<SearchResponse> SearchWebAsycn(
        [Description("The query string to search for.")] string query,
        CancellationToken cancellationToken)
    {
        using var client = new TavilyClient();
        var result = await client.SearchAsync(
            apiKey: Environment.GetEnvironmentVariable("TAVILY_KEY") ?? string.Empty,
            query: query,
            cancellationToken: cancellationToken);
        return result;
    }

    [FunctionTool(Name = "command_line", RequiresApproval = true)]
    [Description("Runs a command and returns the output.")]
    public async Task<string> ExecuteCommandAsync(
        [Description("The command to run in Command Prompt.")] string command,
        CancellationToken cancellationToken)
    {
        // 위험한 명령어 차단
        //string[] blockedCommands = { "rm -rf", "format", "del", "shutdown" };
        //if (blockedCommands.Any(blockedCmd => command.Contains(blockedCmd,
        //    StringComparison.OrdinalIgnoreCase)))
        //{
        //    throw new InvalidOperationException("Potentially dangerous command blocked.");
        //}

        Process process;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            process = new Process
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
        }
        else
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        process.Dispose();

        return string.IsNullOrWhiteSpace(error) ? output : throw new Exception(error);
    }

    [FunctionTool("read_file")]
    [Description("Reads and returns the content of a text file")]
    public async Task<string> ReadFIleAsync(
        [Description("The full path to the file to read")] string filePath,
        CancellationToken cancellationToken)
    {
        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    [FunctionTool(Name = "write_file", RequiresApproval = true)]
    [Description("Saves text content to a specified file")]
    public async Task WriteFIleAsync(
        [Description("The full path where the file should be saved")] string filePath,
        [Description("The text content to write into the file")] string text,
        CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(filePath, text, cancellationToken);
    }
}

