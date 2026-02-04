# 플러그인 시스템

IronHive의 플러그인 아키텍처와 외부 도구 통합 방법입니다.

## 개요

IronHive는 두 가지 플러그인 시스템을 제공하여 LLM이 외부 서비스와 상호작용할 수 있게 합니다:

| 플러그인 | 용도 |
|----------|------|
| **MCP** | Model Context Protocol 기반 도구 서버 연결 |
| **OpenAPI** | REST API 스펙 기반 자동 도구 생성 |

---

## MCP (Model Context Protocol)

**패키지**: `IronHive.Plugins.MCP`

MCP는 LLM과 외부 도구 서버 간의 표준 프로토콜입니다.

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `McpClientManager` | MCP 클라이언트 연결 관리 |
| `McpSession` | 개별 MCP 세션 |
| `McpTool` | MCP 서버에서 노출된 도구 |

### 연결 방식

#### SSE (Server-Sent Events)

HTTP 기반 원격 MCP 서버 연결:

```csharp
var mcpManager = new McpClientManager();

await mcpManager.ConnectAsync("weather-server", new McpSseConfig
{
    Url = "https://mcp.example.com/weather",
    Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer ..."
    }
});
```

#### Stdio (표준 입출력)

로컬 프로세스로 MCP 서버 실행:

```csharp
await mcpManager.ConnectAsync("file-server", new McpStdioConfig
{
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/data"],
    WorkingDirectory = "/app"
});
```

### 도구 사용

```csharp
// 연결된 서버에서 도구 목록 가져오기
var tools = await mcpManager.GetToolsAsync("weather-server");

// HiveService에 도구 등록
foreach (var tool in tools)
{
    builder.AddTool(tool);
}
```

### 세션 관리

```csharp
// 연결 상태 확인
var isConnected = mcpManager.IsConnected("weather-server");

// 연결 해제
await mcpManager.DisconnectAsync("weather-server");

// 모든 연결 해제
await mcpManager.DisconnectAllAsync();
```

### 생명주기

```
ConnectAsync() → GetToolsAsync() → [도구 사용] → DisconnectAsync()
       │                                              │
       └──────────── 연결 유지 (재사용) ──────────────┘
```

---

## OpenAPI 플러그인

**패키지**: `IronHive.Plugins.OpenAPI`

OpenAPI(Swagger) 스펙에서 자동으로 LLM 도구를 생성합니다.

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `OpenApiClientManager` | OpenAPI 클라이언트 관리 |
| `OpenApiClient` | 개별 API 클라이언트 |
| `OpenApiTool` | API 엔드포인트를 도구로 래핑 |

### 클라이언트 생성

```csharp
var apiManager = new OpenApiClientManager();

// OpenAPI 스펙에서 클라이언트 생성
await apiManager.AddClientAsync("petstore", new OpenApiClientConfig
{
    SpecUrl = "https://petstore.swagger.io/v2/swagger.json",
    BaseUrl = "https://petstore.swagger.io/v2",  // 선택사항 (스펙에서 추출)
    Credentials = new ApiKeyCredential
    {
        HeaderName = "api_key",
        ApiKey = "special-key"
    }
});
```

### 인증 방식

```csharp
// API Key
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
// 모든 엔드포인트를 도구로 변환
var tools = await apiManager.GetToolsAsync("petstore");

// 특정 작업만 선택
var tools = await apiManager.GetToolsAsync("petstore", operationIds: ["getPetById", "addPet"]);

// HiveService에 등록
foreach (var tool in tools)
{
    builder.AddTool(tool);
}
```

### 파라미터 직렬화

OpenAPI 도구는 자동으로 파라미터를 적절한 위치에 배치합니다:

- **Path**: URL 경로에 삽입
- **Query**: 쿼리 스트링으로 변환
- **Header**: HTTP 헤더로 추가
- **Body**: JSON 요청 본문으로 직렬화

---

## 커스텀 도구 구현

플러그인 외에도 직접 도구를 구현할 수 있습니다.

### FunctionTool (메서드 기반)

```csharp
public class MyTools
{
    [FunctionTool("calculate_sum")]
    [Description("두 숫자의 합을 계산합니다")]
    public int CalculateSum(
        [Description("첫 번째 숫자")] int a,
        [Description("두 번째 숫자")] int b)
    {
        return a + b;
    }

    [FunctionTool("get_weather")]
    [Description("날씨 정보를 조회합니다")]
    public async Task<WeatherInfo> GetWeather(
        [Description("도시 이름")] string city,
        [FromServices] IWeatherService weatherService)  // DI 주입
    {
        return await weatherService.GetAsync(city);
    }
}

// 등록
var tools = FunctionToolFactory.Create<MyTools>();
foreach (var tool in tools)
{
    builder.AddTool(tool);
}
```

### ITool 인터페이스 직접 구현

```csharp
public class CustomTool : ITool
{
    public string Name => "custom_tool";
    public string Description => "커스텀 도구 설명";
    public ToolSchema Schema => new ToolSchema
    {
        Type = "object",
        Properties = new Dictionary<string, ToolSchemaProperty>
        {
            ["input"] = new() { Type = "string", Description = "입력값" }
        },
        Required = ["input"]
    };

    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var value = input.GetValue<string>("input");
        // 처리 로직
        return ToolOutput.Success(new { result = "처리 완료" });
    }
}
```

---

## 도구 실행 흐름

```
LLM 응답 (tool_use)
       │
       ▼
┌─────────────────────────────────────┐
│         ToolCollection              │
│   이름으로 도구 조회                 │
└─────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────┐
│           Tool.InvokeAsync          │
│   ├─ FunctionTool: 메서드 호출      │
│   ├─ McpTool: MCP 서버 요청         │
│   └─ OpenApiTool: REST API 호출     │
└─────────────────────────────────────┘
       │
       ▼
ToolOutput (결과 또는 에러)
       │
       ▼
LLM에 결과 전달 → 다음 응답 생성
```

---

## 플러그인 선택 가이드

| 시나리오 | 추천 |
|----------|------|
| 기존 REST API 통합 | OpenAPI |
| MCP 호환 도구 서버 | MCP |
| 간단한 로직 | FunctionTool |
| 복잡한 상태 관리 | ITool 직접 구현 |
| 로컬 CLI 도구 | MCP (Stdio) |
