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
    public long Timeout { get; set; }           // 초 단위, 0 이하이면 무제한 (기본값: 0)
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
var instanceTools = FunctionToolFactory.CreateFrom(myToolsInstance);

// Delegate에서 생성
var tool = FunctionToolFactory.CreateFrom(myDelegate, descriptor, serviceProvider);
```

---

## IToolCollection 인터페이스

```csharp
public interface IToolCollection : ICollection<ITool>
{
    IReadOnlyCollection<string> Keys { get; }
    bool TryGet(string key, [MaybeNullWhen(false)] out ITool item);
    bool ContainsKey(string key);
    void AddRange(IEnumerable<ITool> items);
    void Set(ITool item);                                  // 교체
    void SetRange(IEnumerable<ITool> items);                // 일괄 교체
    bool Remove(string key);
    int RemoveAll(Predicate<ITool>? match = null);
    IToolCollection FilterBy(IEnumerable<string> names);    // 이름 필터링
}
```

`Add`/`Contains`/`Clear`(ITool 기준)는 `ICollection<ITool>`에서 상속됩니다.

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
    // cfg.Tools(도구 이름 목록)는 여기서 해석되지 않는다 — 값을 넣으면 NotSupportedException
});

// 도구는 만들어진 에이전트에 직접 설정한다 (일부만 노출하려면 tools.FilterBy(names))
agent.Tools = tools;
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
        if (!input.TryGetValue<string>("query", out var query))
            return ToolOutput.Failure("query is required");

        var results = await _db.QueryAsync(query);
        return ToolOutput.Success(JsonSerializer.Serialize(results));   // 결과는 텍스트(또는 MessageContent 목록)
    }
}
```

### ToolInput / ToolOutput

```csharp
// ToolInput — LLM이 전달한 파라미터 접근 (IReadOnlyDictionary<string, object?>)
input.TryGetValue<string>("name", out var name);        // 없거나 변환 실패면 false
input.TryGetValue<int>("count", out var count);
input.TryGetValue<MyOptions>("options", out var options);
var raw = input["name"];                                 // 원시 값 (없으면 null)

// ToolOutput — 결과 반환
return ToolOutput.Success("처리 완료");
return ToolOutput.Success(JsonSerializer.Serialize(new { id = 42, name = "item" }));
return ToolOutput.Success([new TextMessageContent { Value = "..." }]);   // MessageContent 목록
return ToolOutput.Failure("오류가 발생했습니다");
```

---

## TextCompactor로 도구 출력 압축하기

`IronHive.Core.Utilities.TextCompactor`는 긴 텍스트(도구 출력, 로그 등)를 압축하는 범용
유틸리티입니다. 도구 전용 타입에 묶여 있지 않은 순수 `string → string` 함수라 필요한 곳
어디서든 호출할 수 있습니다. 도구 출력에 적용하려면 **`MessageRequest.ToolOptions.OnAfterInvoke`
에서 직접 연결**합니다.

```csharp
var options = new TextCompactorOptions
{
    EnableJsonToCsv            = true,    // JSON 배열 → CSV 변환 (약 40-50% 토큰 절감)
    JsonToCsvMinElements       = 3,       // CSV 변환 최소 배열 요소 수
    EnableWhitespaceNormalization = true, // 과도한 공백·빈 줄 정규화
    MaxResultChars             = 50_000,  // 최대 출력 문자 수 (초과 시 잘라냄)
    KeepHeadLines              = 100,     // 잘라낼 때 앞에서 유지할 줄 수
    KeepTailLines              = 30       // 잘라낼 때 뒤에서 유지할 줄 수
};

// MessageRequest에 주입
var request = new MessageRequest
{
    Provider = "openai",
    Model    = "gpt-4o",
    Messages = messages,
    Tools    = toolCollection,
    ToolOptions = new ToolOptions
    {
        MaxParallel   = 3,
        Timeout       = TimeSpan.FromSeconds(30),
        OnAfterInvoke = (content, ct) =>
        {
            if (content.Output is { } output)
            {
                var text = string.Join("\n", output.Content.OfType<TextMessageContent>().Select(c => c.Value));
                content.Output = ToolOutput.Success(TextCompactor.Compact(text, options));
            }
            return Task.CompletedTask;
        }
    }
};
```

`ToolOptions`의 전체 구조:

```csharp
public class ToolOptions
{
    public int MaxParallel { get; set; } = 3;       // 병렬 실행 최대 도구 수
    public TimeSpan? Timeout { get; set; }           // 도구 실행 타임아웃 (null = 무제한)

    // 도구 실행 직전 호출. content.Output을 채우면 실제 실행을 스킵(short-circuit)합니다.
    public Func<ToolMessageContent, CancellationToken, Task>? OnBeforeInvoke { get; set; }

    // 도구 실행 직후 호출. content.Output을 직접 수정할 수 있습니다.
    public Func<ToolMessageContent, CancellationToken, Task>? OnAfterInvoke { get; set; }
}
```

---

## 도구 결과 합계 예산 — ToolResultBudgetMiddleware

`OnAfterInvoke`는 결과 **하나**를 다룹니다. 결과마다 상한을 지켜도 도구를 여러 라운드 부르면 결과가
대화에 쌓여, 문맥 창이 작은 모델(로컬 추론 서버의 슬롯 등)에서는 합계가 넘칩니다. `OnAfterInvoke`는
병렬로 호출되므로 호출 사이에 카운터를 둘 수도 없습니다.

합계는 메시지 미들웨어로 제한합니다:

```csharp
builder.AddMessageMiddleware(new ToolResultBudgetMiddleware(maxTotalChars: 12_000));
```

- 매 턴 제너레이터를 부르기 직전에, 이번 호출에서 실행된 도구 결과를 **호출 순서대로** 훑어 예산을
  배분합니다. 예산보다 긴 결과는 남은 만큼으로 잘리고 잘림 표식(`[... truncated by the tool result budget (N chars total) ...]`)이
  붙으며, 남은 예산이 없는 결과는 본문 대신 생략 표식(`[... omitted by the tool result budget (N chars total) ...]`)만 남습니다.
- 예산이 소진된 턴부터는 `ToolChoice.None`으로 요청해 모델이 받은 결과로 답하게 합니다. 그 턴에는
  `ExhaustedNotice`가 호출당 정확히 한 번, 예산을 소진시킨 결과(잘렸든 · 정확히 채웠든 · 생략됐든) 뒤에 별도 텍스트로
  들어갑니다. 결과에 무슨 일이 있었는지는 그 결과의 표식이 말하고 공지는 도구가 사라진 이유만 말하므로, 기본 문구는 어느
  경로에서도 거짓이 아니고(0.26.3 의 «this result was not included» 는 잘린·정확히 채운 경로에서 거짓이었다) 뒤 턴에서 결과가
  더 생략돼도 공지 자리는 옮겨 다니지 않습니다. `ExhaustedNotice` 를 바꿀 때도 결과의 포함 여부를 단정하지 않는 문장을 씁니다.
  이유 없이 도구가 사라지면 모델이 도구 호출을 평문으로 쓸 수 있습니다.
- 텍스트만 셉니다(이미지 등은 그대로 보냅니다). 잘림 표식은 남은 예산 안에 들어가고, 생략 표식과 공지는 예산에 포함되지 않습니다.
- 결과별 압축(`TextCompactor`)과 함께 쓸 수 있습니다 — 압축이 먼저, 예산이 그 다음입니다.
- 버퍼드·스트리밍 호출 모두에 적용됩니다.

---

## 관련 문서

- [PLUGINS.md](PLUGINS.md) — MCP, OpenAPI 플러그인
- [AGENTS.md](AGENTS.md) — 에이전트와 도구 연동
- [SERVICES.md](SERVICES.md) — MessageService 도구 실행 루프
