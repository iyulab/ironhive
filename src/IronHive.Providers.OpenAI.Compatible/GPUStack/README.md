# GPUStack Provider

**공식 문서**: https://docs.gpustack.ai/latest/integrations/openai-compatible-apis/
**Base URL**: `http://localhost:8080` (기본값)
**URL 패턴**: `{BaseUrl}/v1-openai/`
**최종 업데이트**: 2026-05-15

---

## 호환성

- Chat Completions API
- Embeddings API
- Models API

## 특징

### 로컬 실행

GPUStack은 로컬 또는 온프레미스 환경에서 실행되는 GPU 클러스터 관리 플랫폼으로, OpenAI 호환 API를 `/v1-openai/` 경로로 제공합니다.

### API 경로

GPUStack의 OpenAI 호환 엔드포인트는 표준 `/v1` 대신 `/v1-openai/` 경로를 사용합니다.
`GpuStackConfig`는 이 변환을 자동으로 처리합니다.

```
http://localhost:8080/v1-openai/chat/completions
http://localhost:8080/v1-openai/embeddings
http://localhost:8080/v1-openai/models
```

### 커스텀 서버 URL

기본값은 `http://localhost:8080`이지만, 다른 호스트나 포트로 실행 중인 경우 설정을 변경할 수 있습니다.

```csharp
var config = new GpuStackConfig
{
    BaseUrl = "http://192.168.1.100:8080",
    ApiKey = "your-api-key"
};
```

### 동적 엔드포인트·API 키 (런타임 회전)

엔드포인트와 API 키를 시작 시점에 고정하지 않고 **매 요청마다 동적으로** 조회할 수 있습니다.
설정 소스(DB, secret store, 관리자 콘솔)에서 값이 바뀌면 재시작 없이 즉시 반영됩니다.
`BaseUrlResolver`와 `ApiKeyResolver`는 대칭으로 동작하며, resolver가 `null`이거나
빈 문자열을 반환하면 정적 `BaseUrl`/`ApiKey`로 fallback합니다(하위 호환).

```csharp
var config = new GpuStackConfig
{
    // 정적 값은 fallback으로 유지
    BaseUrl = "http://localhost:8080",
    ApiKey = "bootstrap-key",

    // 매 요청 시 평가 — secret rotation / 콘솔 변경이 무중단 반영됨
    BaseUrlResolver = () => settings.GetGpuStackEndpoint(),
    ApiKeyResolver = () => settings.GetGpuStackApiKey(),
};
```

내부 `GpuStackMessageGenerator`는 resolv된 `(BaseUrl, ApiKey)` 시그니처가 바뀔 때에만
inner generator를 재생성하므로, 키만 회전해도 새 키가 정확히 적용됩니다.

## 제한사항

GPUStack은 Chat Completions API 기반(`OpenAIChatMessageGenerator`)으로 동작합니다.
배포된 모델에 따라 지원 기능이 다를 수 있습니다.

---

## 사용 예제

### 기본 사용법

```csharp
using IronHive.Abstractions;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

// GPUStack 설정 - 기본 로컬 서버 사용
var config = new GpuStackConfig
{
    ApiKey = "your-api-key"
};

// 또는 커스텀 서버 URL 사용
var customConfig = new GpuStackConfig
{
    BaseUrl = "http://192.168.1.100:8080",
    ApiKey = "your-api-key"
};

// Hive 서비스 빌더에 추가
builder.AddGpuStackProviders(
    providerName: "gpustack",
    config: config,
    serviceType: GpuStackServiceType.All
);

// 특정 서비스만 추가
builder.AddGpuStackProviders(
    providerName: "gpustack",
    config: config,
    serviceType: GpuStackServiceType.Language | GpuStackServiceType.Embeddings
);
```

### 메시지 생성

```csharp
var request = new MessageGenerationRequest
{
    Model = "your-deployed-model",
    Messages =
    [
        new UserMessage
        {
            Content = [new TextMessageContent { Value = "안녕하세요!" }]
        }
    ]
};

var generator = hiveService.GetMessageGenerator("gpustack");
var response = await generator.GenerateMessageAsync(request);
```

### 임베딩 생성

```csharp
var embeddingGenerator = hiveService.GetEmbeddingGenerator("gpustack");
var embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(
    new[] { "임베딩할 텍스트" },
    model: "your-embedding-model"
);
```

### 사용 가능한 모델 조회

```csharp
var catalog = hiveService.GetModelCatalog("gpustack");
var models = await catalog.GetModelsAsync();
foreach (var model in models)
{
    Console.WriteLine($"Model: {model.Id}");
}
```
