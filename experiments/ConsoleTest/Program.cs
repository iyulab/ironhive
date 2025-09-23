using ConsoleTest;
using IronHive.Abstractions.Tools;
using System.Text.Json;

var config = new OpenApiClientConfig
{
    ClientName = "github",
    Document = File.ReadAllText("Samples/github.json"),
    TimeoutSeconds = 30,
    DefaultHeaders = new Dictionary<string, string>
    {
        ["User-Agent"] = "OpenApiClientTest/1.0"
    },
    Secrets = new Dictionary<string, IOpenApiAuthSecret>
    {
        // 예: Bearer 토큰을 HttpBearerAuthSecret 형태로 제공
        ["BearerAuth"] = new HttpBearerAuthSecret("")
    }
};

var client = await OpenApiClient.CreateAsync(config);
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
    //var result = await tool.InvokeAsync(input);
    //Console.WriteLine($"Result: \n{result.Result}");
}

