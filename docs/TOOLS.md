# 도구 시스템

LLM이 외부 기능을 호출할 수 있도록 하는 도구(Tool) 시스템입니다.

## ITool 인터페이스

```csharp
public interface ITool
{
    string UniqueName { get; }         // LLM이 도구를 식별하는 고유 이름
    string? Description { get; }       // 도구 설명 (LLM이 사용 방법 이해)
    object? Parameters { get; }        // 파라미터 JSON Schema
    bool RequiresApproval { get; }     // 실행 전 승인 필요 여부

    Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default);
}
```

---

## FunctionTool (권장)

`[FunctionTool]` 어트리뷰트로 메서드를 도구로 변환합니다.

### 기본 사용법

```csharp
public class MyTools
{
    [FunctionTool("get_weather", Description = "도시의 날씨를 조회합니다")]
    public async Task<string> GetWeather(
        [Description("도시 이름")] string city)
    {
        // 구현
        return $"{city}의 날씨: 맑음, 23°C";
    }

    [FunctionTool(RequiresApproval = true, Timeout = 30)]
    public string ExecuteScript(
        [Description("실행할 스크립트")] string script)
    {
        // 위험한 작업 — RequiresApproval = true
        return "실행 완료";
    }
}
```

### FunctionToolAttribute 옵션

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class FunctionToolAttribute : Attribute
{
    public string? Name { get; set; }           // null이면 메서드 이름 사용
    public string? Description { get; set; }
    public bool RequiresApproval { get; set; }  // 기본값: false
    public long Timeout { get; set; } = 60;     // 초 단위 (기본값: 60초)
}
```

### DI 주입 파라미터

```csharp
public class MyTools
{
    [FunctionTool]
    public async Task<string> Search(
        string query,
        [FromServices] ISearchService searchService,          // DI 서비스 주입
        [FromKeyedServices("my-key")] IMyService keyedSvc)   // 키 기반 DI 주입
    {
        return await searchService.SearchAsync(query);
    }
}
```

### 반환 타입 지원

```csharp
[FunctionTool] public string Sync() { ... }
[FunctionTool] public Task<string> Async() { ... }
[FunctionTool] public async IAsyncEnumerable<string> Streaming() { ... }
[FunctionTool] public Task<MyObject> Complex() { ... }  // JSON 직렬화됨
```

---

## ToolCollection에 등록

### 타입 기반 등록

```csharp
var tools = new ToolCollection();

// 타입으로 등록 (기본 생성자 사용)
tools.AddFunctionTool<MyTools>();

// 인스턴스로 등록
var instance = new MyTools(dependency);
tools.AddFunctionTool(instance);

// DI와 함께 사용
tools.AddFunctionTool<MyTools>(serviceProvider);
```

### Delegate 기반 등록

```csharp
tools.AddFunctionTool(
    async (string query) => await SearchAsync(query),
    new DelegateDescriptor
    {
        Name = "search",
        Description = "웹을 검색합니다"
    }
);
```

### FunctionToolFactory 직접 사용

```csharp
// 타입에서 모든 [FunctionTool] 메서드 생성
var functionTools = FunctionToolFactory.CreateFrom<MyTools>(serviceProvider);

// 인스턴스에서 생성
var functionTools = FunctionToolFactory.CreateFrom(myToolsInstance);

// Delegate에서 생성
var tool = FunctionToolFactory.CreateFrom(myDelegate, descriptor, serviceProvider);
```

---

## IToolCollection 인터페이스

```csharp
public interface IToolCollection : IEnumerable<ITool>
{
    void Add(ITool tool);
    bool Remove(string uniqueName);
    ITool? Get(string uniqueName);
    bool Contains(string uniqueName);
    IToolCollection FilterBy(IEnumerable<string> names);  // 이름 필터링
    void Set(string uniqueName, ITool tool);               // 교체
    void SetRange(IEnumerable<ITool> tools);               // 일괄 교체
    void Clear();
}
```

---

## 에이전트에 도구 연결

```csharp
var tools = new ToolCollection();
tools.AddFunctionTool<MyTools>();

var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider = "openai";
    cfg.Model = "gpt-4o";
    cfg.Instructions = "도구를 적극적으로 활용하세요.";
    cfg.Tools = tools.Select(t => t.UniqueName).ToList(); // 도구 이름 목록
});

// BasicAgent에 직접 설정
((BasicAgent)agent).Tools = tools;
```

---

## ITool 직접 구현

복잡한 상태 관리나 특수한 직렬화가 필요한 경우:

```csharp
public class DatabaseQueryTool : ITool
{
    private readonly IDbConnection _db;

    public DatabaseQueryTool(IDbConnection db) => _db = db;

    public string UniqueName => "db_query";
    public string? Description => "데이터베이스를 쿼리합니다";
    public bool RequiresApproval => true;

    public object? Parameters => new
    {
        type = "object",
        properties = new
        {
            query = new { type = "string", description = "SQL 쿼리" }
        },
        required = new[] { "query" }
    };

    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var query = input.GetValue<string>("query")
            ?? throw new ArgumentException("query is required");

        var results = await _db.QueryAsync(query);
        return ToolOutput.Success(results);
    }
}
```

### ToolInput / ToolOutput

```csharp
// ToolInput — LLM이 전달한 파라미터 접근
var name = input.GetValue<string>("name");
var count = input.GetValue<int>("count");
var options = input.GetValue<MyOptions>("options");

// ToolOutput — 결과 반환
return ToolOutput.Success("처리 완료");
return ToolOutput.Success(new { id = 42, name = "item" });
return ToolOutput.Error("오류가 발생했습니다");
```

---

## ToolOutputFilter

도구 출력을 LLM에 전달하기 전에 변환·축약하여 토큰 소비를 줄입니다.  
**`MessageRequest.ToolOptions.OutputTransform`에 주입**하여 활성화합니다.

```csharp
var filter = new ToolOutputFilter(new ToolOutputFilterOptions
{
    EnableJsonToCsv            = true,    // JSON 배열 → CSV 변환 (약 40-50% 토큰 절감)
    JsonToCsvMinElements       = 3,       // CSV 변환 최소 배열 요소 수
    EnableWhitespaceNormalization = true, // 과도한 공백·빈 줄 정규화
    MaxResultChars             = 50_000,  // 최대 출력 문자 수 (초과 시 잘라냄)
    KeepHeadLines              = 100,     // 잘라낼 때 앞에서 유지할 줄 수
    KeepTailLines              = 30       // 잘라낼 때 뒤에서 유지할 줄 수
});

// MessageRequest에 주입
var request = new MessageRequest
{
    Provider = "openai",
    Model    = "gpt-4o",
    Messages = messages,
    Tools    = toolCollection,
    ToolOptions = new ToolOptions
    {
        MaxParallel     = 3,
        Timeout         = TimeSpan.FromSeconds(30),
        OutputTransform = filter.Filter   // (toolName, output) => filteredOutput
    }
};
```

`ToolOptions`의 전체 구조:

```csharp
public class ToolOptions
{
    public int MaxParallel { get; set; } = 3;       // 병렬 실행 최대 도구 수
    public TimeSpan? Timeout { get; set; }           // 도구 실행 타임아웃 (null = 무제한)
    public Func<string, ToolOutput, ToolOutput>? OutputTransform { get; set; }  // 출력 변환 델리게이트
}
```

---

## 관련 문서

- [PLUGINS.md](PLUGINS.md) — MCP, OpenAPI 플러그인
- [AGENTS.md](AGENTS.md) — 에이전트와 도구 연동
- [SERVICES.md](SERVICES.md) — MessageService 도구 실행 루프
