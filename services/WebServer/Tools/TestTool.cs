using HtmlAgilityPack;
using IronHive.Core.Tools;
using IronHive.Core.Utilities;
using System.ComponentModel;
using System.Net;
using Tavily;

namespace WebServer.Tools;

public class TestTool
{
    [FunctionTool("extract_web")]
    [Description("Fetches and extracts text content from a web page.")]
    public async Task<object> ExtractWebContentAsync(
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
        var urls = body?.SelectNodes("//a[@href]")?
            .Select(node => node.GetAttributeValue("href", string.Empty))
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .ToList();

        return new
        {
            Title = title,
            Content = content,
            Urls = urls
        };
    }

    [FunctionTool(Name = "search_web", RequiresApproval = true)]
    [Description("Performs a web search and returns the results.")]
    public async Task<SearchResponse> SearchWebAsycn(
        [Description("The query string to search for.")] string query,
        CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);

        using var client = new TavilyClient();
        var result = await client.SearchAsync(
            includeAnswer: true,
            includeRawContent: false,
            apiKey: Environment.GetEnvironmentVariable("TAVILY_KEY") ?? string.Empty,
            query: query,
            cancellationToken: cancellationToken);
        return result;
    }
}

