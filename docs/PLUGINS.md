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

```csharp
var apiManager = new OpenApiClientManager();

await apiManager.AddOrUpdate("petstore", new OpenApiClientOptions
{
    SpecUrl = "https://petstore.swagger.io/v2/swagger.json",
    BaseUrls = ["https://petstore.swagger.io/v2"],  // 순차 시도
    Credential = new ApiKeyCredential
    {
        HeaderName = "api_key",
        ApiKey = "special-key"
    }
});
```

### 인증 방식

```csharp
// API Key (헤더)
new ApiKeyCredential
{
    HeaderName = "X-API-Key",
    ApiKey = "..."
}

// Bearer Token
new BearerTokenCredential
{
    Token = "..."
}

// Basic Auth
new BasicAuthCredential
{
    Username = "user",
    Password = "pass"
}
```

### 도구 사용

```csharp
// 클라이언트에서 도구 목록 조회
var client = apiManager.GetClient("petstore");
var tools = await client.GetToolsAsync();

// 특정 operation만 선택
var tools = await client.GetToolsAsync(operationIds: ["getPetById", "addPet"]);

// 에이전트에 등록
foreach (var tool in tools)
{
    agent.Tools?.Add(tool);
}
```

### IToolCollection에 직접 등록

`OpenApiClientManager`는 `IToolCollection`과 자동 연동 가능합니다:

```csharp
var toolCollection = new ToolCollection();
await apiManager.AddOrUpdate("petstore", options, toolCollection);
// toolCollection에 OpenAPI 도구가 자동으로 추가/갱신됨
```

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
