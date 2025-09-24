using IronHive.Abstractions.Tools;
using IronHive.Plugins.OpenAPI;
using System.Text.Json;

namespace ConsoleTest;

internal class OpenApiTest
{
    public static async Task RunAsync()
    {
        var options = new OpenApiClientOptions
        {
            TimeoutSeconds = 30,
            DefaultHeaders = new Dictionary<string, string>
            {
                ["User-Agent"] = "OpenApiClientTest/1.0"
            },
            Credentials = new Dictionary<string, IOpenApiCredential>
            {
                ["BearerAuth"] = new HttpBearerCredential("")
            }
        };
        var content = File.ReadAllText("Samples/github.json");

        var client = OpenApiClientFactory.CreateFromString("github", content, options);
        var tools = (await client.ListToolsAsync()).ToArray();
        foreach (var tool in tools)
        {
            Console.WriteLine();
            Console.WriteLine($"Tool: {tool.UniqueName} - {tool.Description}");
            Console.WriteLine($"Parameters: {JsonSerializer.Serialize(tool.Parameters, new JsonSerializerOptions { WriteIndented = true })}");
            var input = new ToolInput(new Dictionary<string, object?>
            {
                ["path"] = new
                {
                    owner = "junhyungL",
                    username = "junhyungL",
                    repo = "jsonschema-net"
                },
                ["body"] = new
                {
                    title = "Test Issue from IronHive",
                    body = "This is a test issue created by IronHive OpenAPI client."
                }
            });
            var result = await tool.InvokeAsync(input);
            Console.WriteLine($"Result: \n{result.Result}");
        }
    }
}
