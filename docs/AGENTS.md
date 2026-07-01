# 에이전트 시스템

IronHive의 에이전트는 LLM 기반 대화 및 작업 수행의 핵심 단위입니다.

## 개요

```
IAgent (인터페이스)
    │
    ├── BasicAgent           # 기본 에이전트 구현체
    └── MiddlewareAgent      # 미들웨어 래핑 에이전트 (WithMiddleware 사용 시 자동 생성)

IAgentOrchestrator (인터페이스)
    │
    ├── SequentialOrchestrator   # 순차 실행
    ├── ParallelOrchestrator     # 병렬 실행
    ├── HandoffOrchestrator      # 에이전트 간 전달
    ├── GroupChatOrchestrator    # 그룹 토론
    ├── HubSpokeOrchestrator     # 허브 중심 분배
    └── GraphOrchestrator        # DAG 기반 실행
```

---

## IAgent 인터페이스

```csharp
public interface IAgent
{
    string Provider { get; set; }        // 프로바이더 이름 (예: "openai")
    string Model { get; set; }           // 모델 ID (예: "gpt-4o-mini")
    string Name { get; set; }            // 에이전트 이름
    string Description { get; set; }     // 에이전트 설명
    string? Instructions { get; set; }   // 시스템 프롬프트
    IToolCollection? Tools { get; set; } // 사용 가능한 도구 컬렉션
    int? MaxTokens { get; set; }         // 최대 생성 토큰 수

    Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}
```

---

## 에이전트 생성

### IHiveService를 통한 생성

```csharp
// 프로그래밍 방식
var agent = hive.CreateAgentFrom(config =>
{
    config.Provider = "openai";
    config.Model = "gpt-4o-mini";
    config.Name = "Assistant";
    config.Description = "일반 도우미 에이전트";
    config.Instructions = "당신은 친절한 도우미입니다.";
    config.Parameters = new AgentParametersConfig
    {
        MaxTokens = 4096,
        Temperature = 0.7f
    };
});

// YAML 문자열에서 생성
var agent = hive.CreateAgentFromYaml(yamlString);
```

### AgentConfig 직접 사용

```csharp
public class AgentConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Provider { get; set; }                      // 프로바이더 이름
    public string Model { get; set; }                          // 모델 ID
    public string? Instructions { get; set; }
    public List<string>? Tools { get; set; }                  // 도구 이름 목록 (등록된 도구 참조)
    public Dictionary<string, object?>? ToolOptions { get; set; } // 도구별 설정 옵션
    public AgentParametersConfig? Parameters { get; set; }
}

public class AgentParametersConfig
{
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public float? TopP { get; set; }
    public int? TopK { get; set; }
    public List<string>? StopSequences { get; set; }
}
```

### YAML / TOML / JSON 설정 파일

```yaml
# agent.yaml (root 또는 agent: 하위 모두 지원)
name: "Research Assistant"
provider: "anthropic"
model: "claude-3-5-sonnet-20241022"
instructions: |
  You are a research assistant.
  Always cite your sources.
tools:
  - web_search
  - calculator
parameters:
  maxTokens: 8192
  temperature: 0.3
```

```csharp
var agent = hive.CreateAgentFromYaml(File.ReadAllText("agent.yaml"));
```

TOML / JSON 형식도 동일 구조로 지원:

```csharp
// JSON
var agent = hive.CreateAgentFrom(JsonSerializer.Deserialize<AgentConfig>(json)!);

// TOML — AgentService 사용
var agentService = new AgentService(hive.Messages);
var agent = agentService.CreateAgentFromToml(tomlString);
```

---

## 에이전트 호출

### 동기 호출

```csharp
// Message 목록으로 호출
var messages = new List<Message>
{
    new Message
    {
        Role = MessageRole.User,
        Content = [new TextMessageContent { Value = "안녕하세요" }]
    }
};

var response = await agent.InvokeAsync(messages);

var text = response.Message?.Content
    .OfType<TextMessageContent>()
    .FirstOrDefault()?.Value;
```

### 문자열 오버로드 (AgentExtensions)

```csharp
// 단순 텍스트로 호출 — AgentExtensions 확장 메서드
var response = await agent.InvokeAsync("안녕하세요");
```

### 스트리밍 호출

```csharp
await foreach (var chunk in agent.InvokeStreamingAsync("안녕하세요"))
{
    switch (chunk)
    {
        case StreamingContentDeltaResponse delta:
            if (delta.Delta is TextDeltaContent text)
                Console.Write(text.Value);
            break;
        case StreamingMessageDoneResponse done:
            Console.WriteLine($"\n완료: {done.DoneReason}");
            break;
    }
}
```

---

## MessageResponse

```csharp
public class MessageResponse
{
    public string? ResponseId { get; init; }           // 응답 ID
    public MessageDoneReason? DoneReason { get; init; } // 완료 이유
    public Message? Message { get; init; }             // 생성된 메시지
    public MessageTokenUsage? TokenUsage { get; init; } // 토큰 사용량
    public string Model { get; init; }                 // 사용된 모델
    public DateTime Timestamp { get; init; }           // 응답 시각
}

public enum MessageDoneReason
{
    Stop,      // 정상 완료
    ToolCall,  // 도구 호출 요청
    Length,    // 최대 토큰 도달
    Error      // 오류
}
```

---

## 미들웨어 적용

```csharp
// 단일 미들웨어
var wrapped = agent.WithMiddleware(new RetryMiddleware(maxRetries: 3));

// 다중 미들웨어 (실행 순서: 왼쪽 → 오른쪽)
var wrapped = agent.WithMiddleware(
    new LoggingMiddleware(Console.WriteLine),
    new RetryMiddleware(maxRetries: 3),
    new TimeoutMiddleware(TimeSpan.FromSeconds(30))
);
```

자세한 내용은 [MIDDLEWARE.md](MIDDLEWARE.md)를 참조하세요.

---

## 오케스트레이터를 에이전트로 사용

오케스트레이터를 `IAgent`처럼 다른 오케스트레이터에 중첩할 수 있습니다:

```csharp
var parallelReview = new ParallelOrchestrator();
parallelReview.AddAgent(reviewer1);
parallelReview.AddAgent(reviewer2);

// 오케스트레이터를 IAgent로 래핑
var reviewAgent = parallelReview.AsAgent(name: "ParallelReviewer");

var pipeline = new SequentialOrchestrator();
pipeline.AddAgent(writer);
pipeline.AddAgent(reviewAgent);  // 오케스트레이터를 에이전트로 중첩
pipeline.AddAgent(finalizer);
```

---

## AgentCard

외부 시스템에서 에이전트 정의를 전달할 때 사용합니다:

```csharp
public class AgentCard
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Workflow { get; init; }  // YAML 형식 에이전트 설정
    public string? PromptTemplate { get; init; }
    public object? InitialContext { get; init; }
}

// 사용
var card = new AgentCard
{
    Name = "SupportBot",
    Workflow = yamlConfig
};
var agent = hive.CreateAgentFrom(card);
```

---

## 관련 문서

- [MIDDLEWARE.md](MIDDLEWARE.md) — 미들웨어 시스템 상세
- [ORCHESTRATION.md](ORCHESTRATION.md) — 멀티에이전트 오케스트레이션
- [TOOLS.md](TOOLS.md) — 도구 시스템
