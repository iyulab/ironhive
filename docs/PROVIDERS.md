# AI 프로바이더

IronHive에서 지원하는 AI 프로바이더별 구현 세부사항과 설정 방법입니다.

## 개요

각 프로바이더는 동일한 인터페이스(`IMessageGenerator`, `IEmbeddingGenerator`, `IModelCatalog`)를 구현하여 코드 변경 없이 프로바이더를 교체할 수 있습니다.

## OpenAI

**패키지**: `IronHive.Providers.OpenAI`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `OpenAIChatMessageGenerator` | GPT 모델을 사용한 채팅 완성 |
| `OpenAIResponseMessageGenerator` | Responses API 지원 |
| `OpenAIEmbeddingGenerator` | 텍스트 임베딩 생성 |
| `OpenAIModelCatalog` | 사용 가능한 모델 목록 조회 |

### 등록

```csharp
builder
    .AddMessageGenerator("openai", new OpenAIChatMessageGenerator(new OpenAIConfig
    {
        ApiKey = "sk-...",
        BaseUrl = "https://api.openai.com/v1"  // 선택사항
    }))
    .AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(new OpenAIConfig
    {
        ApiKey = "sk-..."
    }))
    .AddModelCatalog("openai", new OpenAIModelCatalog(new OpenAIConfig
    {
        ApiKey = "sk-..."
    }));
```

### 지원 기능

- 채팅 완성 (Chat Completions API)
- Responses API
- 도구 호출 (Function Calling)
- 스트리밍 응답
- 추론 노력도 설정 (Reasoning Effort)
- 다양한 임베딩 모델 (text-embedding-3-small, text-embedding-3-large 등)

---

## Anthropic

**패키지**: `IronHive.Providers.Anthropic`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `AnthropicMessageGenerator` | Claude 모델을 사용한 메시지 생성 |
| `AnthropicEmbeddingGenerator` | Claude 임베딩 (지원 시) |
| `AnthropicModelCatalog` | Claude 모델 목록 |

### 등록

```csharp
builder
    .AddMessageGenerator("anthropic", new AnthropicMessageGenerator(new AnthropicConfig
    {
        ApiKey = "sk-ant-...",
        BaseUrl = "https://api.anthropic.com"  // 선택사항
    }))
    .AddModelCatalog("anthropic", new AnthropicModelCatalog(new AnthropicConfig
    {
        ApiKey = "sk-ant-..."
    }));
```

### 지원 기능

- Messages API
- 도구 호출
- 스트리밍 응답
- Thinking 콘텐츠 (확장된 사고 과정)
- 시스템 프롬프트
- 멀티모달 입력 (이미지)

---

## Google AI

**패키지**: `IronHive.Providers.GoogleAI`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `GoogleAIMessageGenerator` | Gemini 모델을 사용한 메시지 생성 |
| `GoogleAIEmbeddingGenerator` | Gemini 임베딩 생성 |
| `GoogleAIModelCatalog` | Gemini 모델 목록 |

### 등록

```csharp
builder
    .AddMessageGenerator("google", new GoogleAIMessageGenerator(new GoogleAIConfig
    {
        ApiKey = "AIza..."
    }))
    .AddEmbeddingGenerator("google", new GoogleAIEmbeddingGenerator(new GoogleAIConfig
    {
        ApiKey = "AIza..."
    }))
    .AddModelCatalog("google", new GoogleAIModelCatalog(new GoogleAIConfig
    {
        ApiKey = "AIza..."
    }));
```

### 지원 기능

- Generate Content API
- 도구 호출
- 스트리밍 응답
- 멀티모달 입력 (텍스트, 이미지, 비디오, 오디오)
- 임베딩 생성

---

## Ollama

**패키지**: `IronHive.Providers.Ollama`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `OllamaMessageGenerator` | 로컬 Ollama 모델 사용 |
| `OllamaEmbeddingGenerator` | 로컬 임베딩 생성 |
| `OllamaModelCatalog` | 설치된 모델 목록 |

### 등록

```csharp
builder
    .AddMessageGenerator("ollama", new OllamaMessageGenerator(new OllamaConfig
    {
        BaseUrl = "http://localhost:11434"
    }))
    .AddEmbeddingGenerator("ollama", new OllamaEmbeddingGenerator(new OllamaConfig
    {
        BaseUrl = "http://localhost:11434"
    }))
    .AddModelCatalog("ollama", new OllamaModelCatalog(new OllamaConfig
    {
        BaseUrl = "http://localhost:11434"
    }));
```

### 지원 기능

- 로컬 모델 실행 (Llama, Mistral, Phi 등)
- 도구 호출 (모델 지원 시)
- 스트리밍 응답
- 로컬 임베딩 생성
- 설치된 모델 목록 조회

### 사전 요구사항

1. [Ollama](https://ollama.ai) 설치
2. 원하는 모델 다운로드: `ollama pull llama3`

---

## 프로바이더 선택 가이드

| 요구사항 | 추천 프로바이더 |
|----------|----------------|
| 최고 성능 | OpenAI (GPT-4), Anthropic (Claude 3) |
| 비용 효율 | OpenAI (GPT-4o-mini), Ollama (로컬) |
| 프라이버시 | Ollama (완전 로컬) |
| 멀티모달 | Google AI (Gemini), OpenAI (GPT-4V) |
| 긴 컨텍스트 | Anthropic (Claude), Google AI (Gemini) |

## 공통 생성 파라미터

모든 프로바이더에서 사용 가능한 `GenerationOptions`:

```csharp
var options = new GenerationOptions
{
    Temperature = 0.7f,      // 창의성 (0.0 ~ 2.0)
    MaxTokens = 4096,        // 최대 출력 토큰
    TopP = 0.9f,             // 핵 샘플링
    StopSequences = ["END"], // 중단 시퀀스
    // ... 프로바이더별 추가 옵션
};
```
