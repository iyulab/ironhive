# Tools

## ITool Interface

```csharp
public interface ITool
{
    string UniqueName { get; }
    string Name { get; }
    string? Description { get; }
    bool RequiresApproval { get; }  // default: false (true for MCP/OpenAPI tools)
}
```

## IToolCollection

```csharp
public interface IToolCollection : IEnumerable<ITool>
{
    // Add function tools
    void AddFunctionTool<T>() where T : class;                          // from type (DI-resolved)
    void AddFunctionTool<T>(T instance) where T : class;               // from instance
    void AddFunctionTool(string name, string description,
        Func<string, Task<string>> handler);                           // delegate (single param)

    // Filter / set
    IToolCollection FilterBy(Func<ITool, bool> predicate);
    void Set(ITool tool);
    void SetRange(IEnumerable<ITool> tools);

    // Lookup
    ITool? Get(string uniqueName);
}
```

## [FunctionTool] Attribute

```csharp
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }           // defaults to method name
    public string? Description { get; set; }    // required for LLM to use the tool
    public bool RequiresApproval { get; set; }  // default: false
    public int Timeout { get; set; }            // seconds, default: 60
}
```

## Defining Function Tools

```csharp
public class MyTools
{
    [FunctionTool(Description = "Search the web for a query")]
    public async Task<string> WebSearch(string query, int maxResults = 5)
    {
        // implementation
        return results;
    }

    [FunctionTool(Name = "run_code", Description = "Execute Python code", RequiresApproval = true, Timeout = 120)]
    public string ExecuteCode(string code)
    {
        // implementation
        return output;
    }
}
```

## DI Parameter Injection

```csharp
public class MyTools
{
    // [FromServices] — inject from IServiceProvider
    [FunctionTool(Description = "Fetch URL content")]
    public async Task<string> FetchUrl(
        [FromServices] HttpClient http,
        string url)
    {
        return await http.GetStringAsync(url);
    }

    // [FromKeyedServices] — inject keyed service
    [FunctionTool(Description = "Query database")]
    public async Task<string> Query(
        [FromKeyedServices("main")] IDbConnection db,
        string sql)
    {
        // implementation
    }
}
```

## Registering Tools on an Agent

```csharp
var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider     = "openai";
    cfg.Model        = "gpt-4o";
    cfg.Instructions = "Use tools to answer questions.";
});

agent.Tools ??= new ToolCollection();

// From type (uses DI if ServiceProvider available)
agent.Tools.AddFunctionTool<MyTools>();

// From instance
agent.Tools.AddFunctionTool(new MyTools());

// Single delegate
agent.Tools.AddFunctionTool(
    "calculator",
    "Evaluate a math expression",
    async (string expr) => Evaluate(expr).ToString()
);
```

## MCP Tools

```csharp
using IronHive.Plugins.MCP;

// HTTP transport
var mcpManager = new McpClientManager();
await mcpManager.AddClientAsync("my-server", new McpHttpClientConfig
{
    Endpoint = new Uri("https://mcp.example.com/sse")
});

// Stdio transport
await mcpManager.AddClientAsync("local-server", new McpStdioClientConfig
{
    Command = "npx",
    Args    = ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/files"]
});

// HTTP with OAuth 2.0
await mcpManager.AddClientAsync("oauth-server", new McpHttpClientConfig
{
    Endpoint = new Uri("https://mcp.example.com/sse"),
    OAuth    = new McpHttpOAuthConfig
    {
        ClientId     = "client-id",
        ClientSecret = "client-secret",
        TokenEndpoint = new Uri("https://auth.example.com/token")
    }
});

// Get tools and add to agent
var tools = await mcpManager.GetToolsAsync("my-server");
agent.Tools ??= new ToolCollection();
agent.Tools.SetRange(tools);
```

MCP tool `UniqueName` format: `"mcp_{ServerName}_{ToolName}"`, `RequiresApproval = true`

## OpenAPI Tools

```csharp
using IronHive.Plugins.OpenAPI;

var openApiManager = new OpenApiClientManager();
await openApiManager.AddClientAsync("petstore", new OpenApiClientConfig
{
    SpecUrl   = new Uri("https://petstore.swagger.io/v2/swagger.json"),
    BaseUrl   = "https://petstore.swagger.io/v2"
});

var tools = await openApiManager.GetToolsAsync("petstore");
agent.Tools ??= new ToolCollection();
agent.Tools.SetRange(tools);
```

OpenAPI tool `UniqueName` format: `"openapi_{ClientName}_{OperationId}"`, `RequiresApproval = true`

## Approval Handler

Tools with `RequiresApproval = true` pause execution for human confirmation:

```csharp
// In orchestrator options
WithOptions(new HandoffOrchestratorOptions
{
    ApprovalHandler = async (agent, messages, ct) =>
    {
        Console.Write("Approve tool execution? [y/n]: ");
        return Console.ReadLine() == "y";
    }
})
```

Events emitted: `ApprovalRequired` → `ApprovalGranted` / `ApprovalDenied`

## Custom ITool Implementation

```csharp
public class MyCustomTool : ITool
{
    public string UniqueName    => "my_custom_tool";
    public string Name          => "Custom Tool";
    public string? Description  => "Does something custom";
    public bool RequiresApproval => false;

    public async Task<string> ExecuteAsync(JsonElement arguments)
    {
        var input = arguments.GetProperty("input").GetString();
        return $"Result: {input}";
    }
}
```
