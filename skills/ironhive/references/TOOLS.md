# Tools

## ITool Interface

```csharp
public interface ITool
{
    string UniqueName { get; }      // the name the LLM calls the tool by
    string? Description { get; }
    object? Parameters { get; }     // JSON Schema of the input
    bool RequiresApproval { get; }  // default: false (true for MCP tools)

    Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default);
}
```

## IToolCollection

```csharp
public interface IToolCollection : ICollection<ITool>
{
    IReadOnlyCollection<string> Keys { get; }
    bool TryGet(string key, [MaybeNullWhen(false)] out ITool item);
    bool ContainsKey(string key);
    void AddRange(IEnumerable<ITool> items);
    void Set(ITool item);                                  // add or replace
    void SetRange(IEnumerable<ITool> items);
    bool Remove(string key);
    int RemoveAll(Predicate<ITool>? match = null);
    IToolCollection FilterBy(IEnumerable<string> names);
}
```

```csharp
// Function-tool registration (extension methods, IronHive.Core)
tools.AddFunctionTool<MyTools>(serviceProvider);                 // from type ([FunctionTool] methods; optional DI)
tools.AddFunctionTool(new MyTools());                             // from instance
tools.AddFunctionTool(delegateFn, new DelegateDescriptor { Name = "...", Description = "..." });
```

## [FunctionTool] Attribute

```csharp
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }           // defaults to method name
    public string? Description { get; set; }    // required for LLM to use the tool
    public bool RequiresApproval { get; set; }  // default: false
    public long Timeout { get; set; }           // seconds, default: 60
}
```

## Defining Function Tools

```csharp
public class MyTools
{
    [FunctionTool(Description = "Search the web for a query")]
    public async Task<string> WebSearch(string query, int maxResults = 5)
    {
        await Task.Yield();   // call your search backend here
        return $"top {maxResults} results for '{query}'";
    }

    [FunctionTool(Name = "run_code", Description = "Execute Python code", RequiresApproval = true, Timeout = 120)]
    public string ExecuteCode(string code)
    {
        return $"ran {code.Length} characters";   // run the code in your sandbox here
    }
}
```

## DI Parameter Injection

```csharp
public class DataTools
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
    public Task<string> Query(
        [FromKeyedServices("main")] System.Data.IDbConnection db,
        string sql)
    {
        using var command = db.CreateCommand();
        command.CommandText = sql;
        return Task.FromResult(Convert.ToString(command.ExecuteScalar()) ?? "");
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

// From type (pass an IServiceProvider for [FromServices] parameters)
agent.Tools.AddFunctionTool<MyTools>();

// From instance
agent.Tools.AddFunctionTool(new MyTools());

// Single delegate
agent.Tools.AddFunctionTool(
    (string text) => text.ToUpperInvariant(),
    new DelegateDescriptor { Name = "uppercase", Description = "Upper-case a text" });
```

## MCP Tools

`McpClientManager` keeps the tools of every connected server in the `IToolCollection` it was
constructed with: they are added when a session connects and removed when it disconnects or errors.

```csharp
using IronHive.Plugins.MCP;

agent.Tools ??= new ToolCollection();
var mcpManager = new McpClientManager(agent.Tools);

// HTTP transport (SSE / Streamable HTTP) — AddOrUpdate is synchronous; the session connects in the background
mcpManager.AddOrUpdate(new McpHttpClientConfig
{
    ServerName = "my-server",
    Endpoint   = new Uri("https://mcp.example.com/mcp")
});

// Stdio transport
mcpManager.AddOrUpdate(new McpStdioClientConfig
{
    ServerName = "local-server",
    Command    = "npx",
    Arguments  = ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/files"]
});

// HTTP with OAuth 2.0 (endpoints are discovered from the server's metadata)
mcpManager.AddOrUpdate(new McpHttpClientConfig
{
    ServerName = "oauth-server",
    Endpoint   = new Uri("https://mcp.example.com/mcp"),
    OAuth      = new McpHttpOAuthConfig
    {
        RedirectUri  = new Uri("http://localhost:8080/callback"),   // required
        ClientId     = "client-id",        // null = Dynamic Client Registration
        ClientSecret = "client-secret"     // optional with PKCE
    }
});

// One server's tools, read directly from its session
var session = mcpManager.GetSession("my-server");
IEnumerable<McpTool> tools = session is null ? [] : await session.ListToolsAsync();
```

MCP tool `UniqueName` format: `"mcp_{ServerName}_{ToolName}"`, `RequiresApproval = true`

## ToolOptions — Intercepting Invocation

```csharp
// ToolOptions: MaxParallel (default 3, concurrent tool executions) · Timeout (per tool, null = unlimited)
//   · OnBeforeInvoke (right before invocation; pre-filling content.Output short-circuits the real call)
//   · OnAfterInvoke (right after invocation, success or failure; mutate content.Output directly)
var request = new MessageRequest
{
    Provider = "openai",
    Model    = "gpt-4o",
    Messages = messages,
    Tools    = tools,
    ToolOptions = new ToolOptions
    {
        MaxParallel   = 3,
        Timeout       = TimeSpan.FromSeconds(30),
        OnAfterInvoke = (content, ct) =>
        {
            if (content.Output is { } output)
            {
                var text = string.Join("\n", output.Content.OfType<TextMessageContent>().Select(c => c.Value));
                content.Output = ToolOutput.Success(TextCompactor.Compact(text));
            }
            return Task.CompletedTask;
        }
    }
};
```

`IronHive.Core.Utilities.TextCompactor` is a plain `string → string` utility (JSON→CSV,
whitespace normalization, truncation) for shrinking large tool outputs — not bound to
`ToolOutput`, so it can be called from anywhere, `OnAfterInvoke` being the typical spot.

`OnAfterInvoke` sees one result and runs in parallel, so it cannot bound the **total** that
builds up across tool rounds. For that, register the message middleware
`IronHive.Core.Services.ToolResultBudgetMiddleware`:

```csharp
builder.AddMessageMiddleware(new ToolResultBudgetMiddleware(maxTotalChars: 12_000));
```

Before every turn it shares the budget out over this call's tool results in call order — a
result longer than what is left is cut, one with nothing left becomes `ExhaustedNotice` — and
once the budget is spent it requests `ToolChoice.None`. Text only; buffered and streaming.

## Approval

Calls to a tool with `RequiresApproval = true` come back as a `ToolMessageContent` with
`IsApproved = false`. `MessageService` does not run unapproved calls; it ends the loop and returns
the assistant message with the pending call. To continue, set `IsApproved = true` on the calls you
approve, append that assistant message to `Messages`, and call again — the next call runs the
approved tools before generating.

```csharp
var response = await hive.Messages.GenerateMessageAsync(request);

var pending = response.Message?.Content.OfType<ToolMessageContent>().Where(t => !t.IsApproved).ToList() ?? [];
if (pending.Count > 0)
{
    foreach (var call in pending)
    {
        Console.Write($"Approve {call.Name}({call.Input})? [y/n]: ");
        call.IsApproved = Console.ReadLine() == "y";
    }
    request.Messages.Add(response.Message!);
    response = await hive.Messages.GenerateMessageAsync(request);
}
```

Orchestrators have a separate, per-agent gate — `ApprovalHandler` on the options, or
`SetApprovalHandler((agentName, previousStep) => ...)` on the Handoff/GroupChat builders — which emits
`ApprovalRequired` → `ApprovalGranted` / `ApprovalDenied` (see [ORCHESTRATION.md](ORCHESTRATION.md)).

## Custom ITool Implementation

```csharp
public class MyCustomTool : ITool
{
    public string UniqueName     => "my_custom_tool";
    public string? Description   => "Does something custom";
    public bool RequiresApproval => false;

    public object? Parameters => new
    {
        type = "object",
        properties = new { input = new { type = "string" } },
        required = new[] { "input" }
    };

    public Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(input.TryGetValue<string>("input", out var value)
            ? ToolOutput.Success($"Result: {value}")
            : ToolOutput.Failure("input is required"));
    }
}
```
