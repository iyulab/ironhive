# 플러그인 시스템

외부 서비스를 LLM 도구로 연결하는 플러그인 시스템입니다.

## 개요

| 플러그인 | 용도 |
|----------|------|
| **MCP** | Model Context Protocol 기반 도구 서버 연결 |

---

## MCP (Model Context Protocol)

**패키지**: `IronHive.Plugins.MCP`

**지원 규격**: MCP **2026-07-28** 리비전(ModelContextProtocol SDK 2.x). 연결 시 SDK가 `server/discover`로 먼저
협상하고, 그 리비전 이전의 서버에는 `initialize` 핸드셰이크로 폴백합니다 — 협상 결과는
`McpSession.NegotiatedProtocolVersion`으로 볼 수 있습니다. 2026-07-28 리비전은 `ping`을 제거했으므로
`HealthAsync`는 협상된 리비전에 맞는 유틸리티(`server/discover` 또는 구형 서버의 `ping`)로 생존을 확인합니다.

### 연결 방식

#### HTTP (SSE / Streamable HTTP)

HTTP 기반 원격 MCP 서버. SSE와 Streamable HTTP를 자동으로 감지하여 연결합니다.

`McpClientManager`는 연결된 서버의 도구를 **생성자로 받은 `IToolCollection`에 자동 반영**한다.
`AddOrUpdate`는 기다릴 것 없이(void) 돌아오고 연결은 백그라운드에서 진행된다 — 연결되면 그 서버의 도구가
컬렉션에 등록되고, 연결이 끊기거나 오류가 나면 제거된다.

```csharp
var tools = new ToolCollection();
var mcpManager = new McpClientManager(tools);

mcpManager.AddOrUpdate(new McpHttpClientConfig
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
mcpManager.AddOrUpdate(new McpHttpClientConfig
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
mcpManager.AddOrUpdate(new McpStdioClientConfig
{
    ServerName = "file-server",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/data"],
    WorkingDirectory = "/app",
    EnvironmentVariables = new Dictionary<string, string?>
    {
        ["NODE_ENV"] = "production"
    },
    ShutdownTimeout = TimeSpan.FromSeconds(5)
});
```

### 세션 및 도구 관리

```csharp
// 세션 조회 (등록되지 않은 이름이면 false)
if (!mcpManager.TryGetSession("weather-server", out var session))
    return;

// 도구 목록 조회 — 아직 연결 전이면 빈 목록이다
var mcpTools = await session.ListToolsAsync();

// 에이전트에 도구 추가 (연결 상태를 따라가게 하려면 매니저의 컬렉션을 그대로 준다: agent.Tools = tools;)
foreach (var tool in mcpTools)
{
    agent.Tools?.Add(tool);
}

// 협상된 규격 리비전과 스펙 기준 생존 확인 (2026-07-28 서버는 server/discover, 구형 서버는 ping)
Console.WriteLine(session.NegotiatedProtocolVersion);   // "2026-07-28" 또는 "2025-11-25" 등
var alive = await session.HealthAsync();
```

### McpTool 특성

- `UniqueName`: `"mcp_{ServerName}_{ToolName}"` 형식
- `RequiresApproval`: 기본값 `true`
- JSON 폴리모픽 타입: `"mcp"`
- 실패 의미: 서버가 `isError`로 답하면 그 내용이, 전송·프로토콜 실패(서버 프로세스 사망, 연결 거부/리셋,
  프레임 파싱 실패)면 서버·도구 이름을 담은 메시지가 `ToolOutput.Failure`로 모델에 전달된다 — `FunctionTool`이
  던지는 메서드에 주는 결과와 같다. 호출자 자신의 취소(`OperationCanceledException`)만 그대로 전파된다.

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
    public object? Parameters => new { type = "object", properties = new { input = new { type = "string" } } };   // JSON Schema
    public bool RequiresApproval => false;

    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        if (!input.TryGetValue<string>("input", out var value))
            return ToolOutput.Failure("'input' 인자가 필요합니다");
        return ToolOutput.Success($"처리 완료: {value}");
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
IToolCollection.TryGet(name, out tool)
       │
       ▼
ITool.InvokeAsync(input)
    ├─ FunctionTool: 메서드 호출
    └─ McpTool:      MCP 서버 요청
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
| MCP 호환 도구 서버 | MCP (HTTP) |
| 로컬 CLI 도구 | MCP (Stdio) |
| 인증 필요 원격 MCP | MCP (HTTP + OAuth) |
| 간단한 비즈니스 로직 | FunctionTool |
| 복잡한 상태 관리 | ITool 직접 구현 |

---

## 관련 문서

- [TOOLS.md](TOOLS.md) — FunctionTool 상세
- [AGENTS.md](AGENTS.md) — 에이전트와 도구 연동
