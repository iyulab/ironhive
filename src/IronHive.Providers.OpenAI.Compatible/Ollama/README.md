# Ollama Provider

**공식 문서**: https://docs.ollama.com/api/openai-compatibility
**Base URL**: `http://localhost:11434/v1` (기본값)
**최종 업데이트**: 2026-02-13

---

## 호환성

- Responses API (Beta)
- Embeddings API
- Models API

## 특징

### 로컬 실행

Ollama는 로컬에서 실행되는 LLM 서버로, API 키가 필요하지 않습니다.
OpenAI 클라이언트 호환성을 위해 더미 값("ollama")을 사용합니다.

### 모델 관리

`ollama pull <model>` 명령어로 모델을 다운로드한 후 사용할 수 있습니다.

```bash
# 예: Llama 3.1 모델 다운로드
ollama pull llama3.1
```

### 커스텀 서버 URL

기본값은 `http://localhost:11434/v1`이지만, 다른 호스트나 포트로 실행 중인 경우 설정을 변경할 수 있습니다.

```csharp
var config = new OllamaConfig
{
    BaseUrl = "http://192.168.1.100:11434/v1"
};
```

## 제한사항

### Responses API

다음 파라미터는 지원되지 않으며 요청 시 제거됩니다:

- `top_logprobs` - 로그 확률 관련 파라미터
- `stream_options` - 스트리밍 옵션 (일부 버전에서 미지원)

### Response Format

- `json_object` 형식만 부분적으로 지원
- `json_schema`는 미지원

### Tool Calls

- 기본 tool calling은 지원하지만 일부 고급 기능은 제한될 수 있음
- 모델에 따라 지원 여부가 다를 수 있음

---

## 사용 예제

### 기본 사용법

```csharp
using IronHive.Abstractions;
using IronHive.Providers.OpenAI.Compatible.Ollama;

// Ollama 설정 - 기본 로컬 서버 사용
var config = new OllamaConfig();

// 또는 커스텀 서버 URL 사용
var customConfig = new OllamaConfig
{
    BaseUrl = "http://192.168.1.100:11434/v1"
};

// Hive 서비스 빌더에 추가
builder.AddOllamaProviders(
    providerName: "ollama",
    config: config,
    serviceType: OllamaServiceType.All
);

// 특정 서비스만 추가
builder.AddOllamaProviders(
    providerName: "ollama",
    config: config,
    serviceType: OllamaServiceType.Language | OllamaServiceType.Embeddings
);
```

### 메시지 생성

```csharp
var request = new MessageGenerationRequest
{
    Model = "llama3.1",  // ollama pull llama3.1로 다운로드한 모델
    Messages = 
    [
        new UserMessage
        {
            Content = [new TextMessageContent { Value = "안녕하세요!" }]
        }
    ]
};

var generator = hiveService.GetMessageGenerator("ollama");
var response = await generator.GenerateMessageAsync(request);
```

### 임베딩 생성

```csharp
var embeddingGenerator = hiveService.GetEmbeddingGenerator("ollama");
var embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(
    new[] { "임베딩할 텍스트" },
    model: "nomic-embed-text"  // ollama pull nomic-embed-text
);
```

### 사용 가능한 모델 조회

```csharp
var catalog = hiveService.GetModelCatalog("ollama");
var models = await catalog.GetModelsAsync();
foreach (var model in models)
{
    Console.WriteLine($"Model: {model.Id}");
}
```
