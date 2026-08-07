# 플러그인 시스템

외부 서비스를 LLM 도구로 연결하는 두 가지 플러그인 시스템입니다.

## 개요

| 플러그인 | 용도 |
|----------|------|
| **MCP** | Model Context Protocol 기반 도구 서버 연결 |
| **OpenAPI** | REST API 스펙에서 도구 자동 생성 |

---

## MCP (Model Context Protocol)

**패키지**: `IronHive.Plugins.MCP`

### 연결 방식

#### HTTP (SSE / Streamable HTTP)

HTTP 기반 원격 MCP 서버. SSE와 Streamable HTTP를 자동으로 감지하여 연결합니다.

```csharp
var mcpManager = new McpClientManager();

await mcpManager.AddOrUpdate(new McpHttpClientConfig
{
    ServerName = "weather-server",
    Endpoint = new Uri("https://mcp.example.com/mcp"),
    AdditionalHeaders = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer ..."
    },
    ConnectionTimeout = TimeSpan.FromSeconds(30)
});
```

#### OAuth 2.0 인증 (HTTP)

```csharp
await mcpManager.AddOrUpdate(new McpHttpClientConfig
{
    ServerName = "secure-server",
    Endpoint = new Uri("https://mcp.example.com/mcp"),
    OAuth = new McpHttpOAuthConfig
    {
        RedirectUri = new Uri("http://localhost:8080/callback"),
        ClientId = "my-client-id",       // null이면 Dynamic Client Registration
        ClientSecret = "...",            // PKCE 사용 시 생략 가능
        Scopes = ["read", "write"],      // null이면 서버 기본값 사용
    }
});
```

#### Stdio (표준 입출력)

로컬 프로세스로 MCP 서버를 실행합니다:

```csharp
await mcpManager.AddOrUpdate(new McpStdioClientConfig
{
    ServerName = "file-server",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/data"],
    WorkingDirectory = "/app",
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["NODE_ENV"] = "production"
    },
    ShutdownTimeout = TimeSpan.FromSeconds(5)
});
```

### 세션 및 도구 관리

```csharp
// 세션 조회
var session = mcpManager.GetSession("weather-server");

// 도구 목록 조회
var tools = await session.GetToolsAsync();

// 에이전트에 도구 추가
foreach (var tool in tools)
{
    agent.Tools?.Add(tool);
}
```

### McpTool 특성

- `UniqueName`: `"mcp_{ServerName}_{ToolName}"` 형식
- `RequiresApproval`: 기본값 `true`
- JSON 폴리모픽 타입: `"mcp"`

---

## OpenAPI 플러그인

**패키지**: `IronHive.Plugins.OpenAPI`

OpenAPI(Swagger) 스펙에서 자동으로 LLM 도구를 생성합니다.

### 클라이언트 생성

`OpenApiClientManager`는 등록된 클라이언트의 도구를 **생성자로 받은 `IToolCollection`에 자동 반영**한다.
스펙 파싱은 호출자가 한다 — 옵션에 스펙 URL을 넣는 자리는 없고, `Microsoft.OpenApi`로 읽은
`OpenApiDocument`를 넘긴다.

```csharp
var tools = new ToolCollection();
var apiManager = new OpenApiClientManager(tools);

// 스펙을 읽어 OpenApiDocument로 만든다 (Microsoft.OpenApi)
using var stream = await new HttpClient().GetStreamAsync(
    "https://petstore.swagger.io/v2/swagger.json");
var result = await OpenApiDocument.LoadAsync(stream);

var client = new OpenApiClient(result.Document!, new OpenApiClientOptions
{
    // 키는 스펙의 security scheme 이름이다
    Credentials = new Dictionary<string, IOpenApiCredential>
    {
        ["api_key"] = new ApiKeyCredential("special-key")
    },
    TimeoutSeconds = 60,
})
{
    ClientName = "petstore"   // required
};

apiManager.AddOrUpdate(client);   // tools 컬렉션에 도구가 등록된다
```

요청을 보낼 base URL은 옵션이 아니라 **스펙의 `servers`** 에서 온다 — operation → path item →
document 순으로 처음 발견된 것을 쓴다.

### 인증 방식

전부 **positional record** 이며, 자격증명은 스펙의 security scheme 이름으로 매핑한다.
헤더·쿼리·경로·쿠키 중 어디에 실릴지는 자격증명이 아니라 **스펙의 `in`/`name`** 이 정한다.

```csharp
new ApiKeyCredential("...")                  // apiKey — 위치는 스펙이 정한다
new HttpBearerCredential("...")              // http, scheme: bearer
new HttpBasicCredential("user", "pass")      // http, scheme: basic
new OAuth2Credential("...")                  // oauth2 (access token)
new OpenIdConnectCredential("...")           // openIdConnect (access token)
```

각 타입의 `Match(scheme)`가 스펙의 스킴과 맞는지 검사하므로, 스킴 이름이 같아도 종류가 다르면 적용되지 않는다.

### 도구 사용

```csharp
if (apiManager.TryGetClient("petstore", out var client))
{
    var apiTools = await client.ListToolsAsync();

    foreach (var tool in apiTools)
        agent.Tools?.Add(tool);
}

// 등록된 전체 클라이언트
foreach (var c in apiManager.Clients)
    Console.WriteLine($"{c.ClientName}: {c.Title}");

// 제거 — 해당 클라이언트의 도구도 함께 정리된다
apiManager.Remove("petstore");
```

`ListToolsAsync`는 스펙의 모든 operation을 도구로 만든다(첫 호출 결과를 캐시한다).
일부만 노출하려면 도구 컬렉션에 넣을 때 걸러낸다.

### OpenApiTool 특성

- `UniqueName`: `"openapi_{ClientName}_{OperationId}"` 형식
- `RequiresApproval`: 기본값 `true`
- 파라미터 위치 자동 처리: Path / Query / Header / Body

---

## 커스텀 도구 구현

플러그인 외에도 직접 도구를 구현할 수 있습니다.

### FunctionTool (권장)

```csharp
public class MyTools
{
    [FunctionTool("calculate_sum", Description = "두 숫자의 합을 계산합니다")]
    public int CalculateSum(
        [Description("첫 번째 숫자")] int a,
        [Description("두 번째 숫자")] int b)
    => a + b;

    [FunctionTool(RequiresApproval = true, Timeout = 30)]
    public async Task<string> ExecuteCode(
        string code,
        [FromServices] ICodeRunner runner)   // DI 주입
    => await runner.RunAsync(code);
}

// ToolCollection에 등록
var tools = new ToolCollection();
tools.AddFunctionTool<MyTools>();  // 또는 .AddFunctionTool(instance)
```

### ITool 직접 구현

```csharp
public class CustomTool : ITool
{
    public string UniqueName => "custom_tool";
    public string? Description => "커스텀 도구 설명";
    public object? Parameters => /* JSON Schema */;
    public bool RequiresApproval => false;

    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var value = input.GetValue<string>("input");
        return ToolOutput.Success(new { result = "처리 완료" });
    }
}
```

자세한 내용은 [TOOLS.md](TOOLS.md)를 참조하세요.

---

## 도구 실행 흐름

```
LLM 응답 (tool_use)
       │
       ▼
IToolCollection.GetTool(name)
       │
       ▼
ITool.InvokeAsync(input)
    ├─ FunctionTool: 메서드 호출
    ├─ McpTool:      MCP 서버 요청
    └─ OpenApiTool:  REST API 호출
       │
       ▼
ToolOutput (성공/오류)
       │
       ▼
LLM에 결과 전달 → 다음 응답 생성
```

---

## 플러그인 선택 가이드

| 시나리오 | 추천 |
|----------|------|
| 기존 REST API 통합 | OpenAPI |
| MCP 호환 도구 서버 | MCP (HTTP) |
| 로컬 CLI 도구 | MCP (Stdio) |
| 인증 필요 원격 MCP | MCP (HTTP + OAuth) |
| 간단한 비즈니스 로직 | FunctionTool |
| 복잡한 상태 관리 | ITool 직접 구현 |

---

## 관련 문서

- [TOOLS.md](TOOLS.md) — FunctionTool 상세
- [AGENTS.md](AGENTS.md) — 에이전트와 도구 연동
