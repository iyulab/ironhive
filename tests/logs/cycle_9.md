# Cycle 9: Multimodal Image Input Verification

## 범위

이미지 입력 (멀티모달) 검증. Provider별 이미지+텍스트 혼합 요청의 SDK 동작 확인. 기본/스트리밍 두 시나리오 추가.

### 프로젝트 철학 정렬
- **빌드타임 안전성**: `ImageMessageContent`의 `Format` enum이 타입 안전하게 이미지 형식을 제한
- **Provider 추상화**: 동일한 `ImageMessageContent`로 OpenAI, Anthropic, Google, xAI 모두 동작
- **SDK 사용자 가치**: 멀티모달이 Provider-agnostic하게 동작하는지 실증

## SDK 수정 사항

### 수정 1: OpenAI Provider — base64 → data URI 변환

- **파일**: `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs`
- **문제**: `ImageUrl = image.Base64`로 raw base64 문자열을 전달 → OpenAI API가 "Expected a valid URL" 에러 반환
- **수정**: `ToDataUri()` 헬퍼 메서드 추가, `ImageFormat`에서 MIME type 매핑 후 `data:{mime};base64,{base64}` 형식으로 변환
- **영향 범위**: Responses API 경로 + Chat Completion API 경로 모두 수정
- **하위호환**: Anthropic/Google은 별도 형식 사용이므로 영향 없음

```csharp
// 추가된 헬퍼
private static string ToDataUri(ImageMessageContent image)
{
    var mime = image.Format switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.Gif => "image/gif",
        ImageFormat.Webp => "image/webp",
        _ => throw new NotSupportedException(...)
    };
    return $"data:{mime};base64,{image.Base64}";
}
```

**참고**: Anthropic/Google Provider는 이미 올바른 형식 사용 중
- Anthropic: `Base64ImageSource { MediaType, Data }` — MIME type + raw base64 분리 전달
- Google: `BlobData { MimeType, Data }` — 동일 패턴

## 추가된 시나리오

### 17. image-input (Image + Text Basic)
- 테스트 이미지 (`tests/multimodal-test-image-1.png`) 로드 → base64 인코딩
- `ImageMessageContent(Format=Png)` + `TextMessageContent("Describe what this image shows")` 혼합 UserMessage
- 응답 존재 확인 + 관련 키워드 매칭 (map, boundary, water, trip, earth, route, island, lake, trail, day, legend, satellite)
- 비전 미지원 모델은 이미지를 해석하지 못하더라도 SDK 에러 없이 응답 → PASS

### 18. image-stream (Image + Text Streaming)
- 동일 이미지 입력, 스트리밍 모드로 응답 수신
- `StreamingMessageBeginResponse` → `StreamingContentDeltaResponse` → `StreamingMessageDoneResponse` 라이프사이클 검증
- 텍스트 수집 확인

## 실행 결과

**89 PASS / 3 SKIP / 1 FAIL** (FAIL은 기존 gpustack/tool-roundtrip 이슈)

| Provider | 기존 16개 | image-input | image-stream | 합계 |
|----------|-----------|-------------|--------------|------|
| openai (o4-mini) | 16/16 PASS | PASS | PASS | 18/18 |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | 16/16 PASS | PASS | PASS | 18/18 |
| google (gemini-2.5-flash) | 16/16 PASS | PASS | PASS | 18/18 |
| xai (grok-4-1-fast-reasoning) | 16/16 PASS | PASS | PASS | 18/18 |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | 15/16 PASS* | PASS** | PASS** | 17/18 |

\* tool-roundtrip: 기존 모델 특정 이슈 (message header parsing error)
\** gpustack: 비전 미지원 모델이므로 이미지 내용을 인식하지 못함. 그러나 SDK 에러 없이 요청/응답 정상 처리

### Provider별 이미지 인식 결과

| Provider | 인식된 키워드 | 응답 길이 | 비고 |
|----------|-------------|-----------|------|
| openai | map,boundary,water,earth,route,island,day,legend,satellite | 443자 | 정확한 설명 |
| anthropic | map,boundary,water,trip,route,island,lake,day,satellite | 406자 | 정확한 설명 |
| google | map,boundary,water,trip,earth,route,island,day,legend | 309자 | 정확한 설명 |
| xai | map,boundary,water,earth,route,island,lake,satellite | 480자 | 정확한 설명 |
| gpustack | (없음) | 123자 | "I can't see the image" — 비전 미지원 |

### 스트리밍 이미지 Provider별 특성

| Provider | begin | delta | done | chunks |
|----------|-------|-------|------|--------|
| openai | true | true | true | 37 |
| anthropic | true | true | true | 27 |
| google | true | **false** | true | 4 |
| xai | true | true | true | 38 |
| gpustack | true | true | true | 213 |

**Google 관찰**: `delta=False` — Google은 텍스트를 `StreamingContentAddedResponse`로 전달하고 Delta를 사용하지 않음 (기존 streaming 시나리오와 동일한 패턴). SDK가 양쪽 모두 처리하므로 정상.

## 검증

```
# 빌드: 0 Error
dotnet build IronHive.slnx

# data URI 변환이 Responses/Chat 양 경로에 적용됨
grep ToDataUri MessageGenerationRequestExtensions.cs → 2건 매칭

# 전체 테스트: 89 PASS / 3 SKIP / 1 FAIL (기존 이슈)
tests/chat-api-tests/run.ps1
```

## 변경된 파일

| 파일 | 작업 |
|------|------|
| `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` | `ToDataUri()` 헬퍼 추가, Responses/Chat 양 경로에서 호출 |
| `tests/chat-api-tests/Program.cs` | `TestImageInput`, `TestImageStream` 시나리오 추가 |

## 누적 시나리오 현황

| # | 시나리오 | 검증 대상 | 도입 사이클 |
|---|----------|-----------|-------------|
| 1 | basic | 기본 생성, DoneReason, TokenUsage, Thinking | C1 |
| 2 | streaming | 스트리밍 이벤트 라이프사이클, 텍스트 수집 | C1 |
| 3 | tool-call | 도구 호출 감지, ToolMessageContent 반환 | C2 |
| 4 | tool-roundtrip | 도구 실행 → 결과 반환 → 최종 응답 | C3 |
| 5 | multi-turn | 대화 컨텍스트 유지 (2턴) | C4 |
| 6 | tool-params | JSON Schema 파라미터 + 인자 바인딩 | C5 |
| 7 | stream-tool | 스트리밍 도구 호출 이벤트 수신 | C5 |
| 8 | multi-tool | 다중 도구 중 올바른 선택 | C5 |
| 9 | think-none | ThinkingEffort None 추론 비활성화 | C5 |
| 10 | max-tokens | MaxTokens 출력 절단 + DoneReason.MaxTokens | C6 |
| 11 | think-high | ThinkingEffort High 깊은 추론 | C6 |
| 12 | stop-seq | StopSequences 생성 중단 조건 | C6 |
| 13 | stream-roundtrip | 스트리밍 도구 호출 → 실행 → 스트리밍 연속 | C6 |
| 14 | error-model | 잘못된 모델명 에러 핸들링 | C7 |
| 15 | no-system | SystemPrompt 없는 요청 | C7 |
| 16 | temperature | Temperature=0 결정론적 출력 | C7 |
| 17 | image-input | 이미지+텍스트 멀티모달 기본 | C9 |
| 18 | image-stream | 이미지+텍스트 멀티모달 스트리밍 | C9 |

## 도출된 SDK 이슈

### 해결됨
| 이슈 | 상태 | 해결 방법 |
|------|------|-----------|
| OpenAI Provider 이미지 base64가 data URI 형식이 아님 | **해결** | `ToDataUri()` 헬퍼 추가, MIME type 매핑 |

### 관찰 사항 (수정 불필요)
| 관찰 | 설명 |
|------|------|
| gpustack 비전 미지원 | gpt-oss-20b는 이미지를 처리하지 못함. SDK 에러 없이 정상 동작하므로 SDK 이슈 아님 |
| Google 스트리밍 delta 미사용 | Google은 ContentAdded로 텍스트를 전달. 기존 패턴과 동일, SDK가 처리함 |

## 다음 사이클

- [ ] parallel tool calls (병렬 도구 호출 지원 여부)
- [ ] multiple user messages in conversation (3턴 이상 복잡 대화)
- [ ] large context / long prompt 처리
- [ ] TopP / TopK 파라미터 검증
- [ ] document input (DocumentMessageContent 검증)
- [ ] multiple images in single message (다중 이미지)
