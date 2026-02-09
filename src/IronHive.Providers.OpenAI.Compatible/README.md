# IronHive.Providers.OpenAI.Compatible

OpenAI API와 호환되는 다양한 서비스 제공자를 지원하는 패키지입니다.
각 Provider의 상세 특이사항은 해당 폴더의 README.md를 참고하세요.

## 새 Provider 추가 가이드

새로운 OpenAI 호환 Provider를 추가할 때 다음 단계를 따릅니다.

### 1. 폴더 생성

프로젝트 루트에 Provider 이름으로 폴더를 생성합니다.

### 2. README.md 작성

각 폴더에 README.md를 작성합니다. 포함해야 할 내용:

- 공식 문서 링크
- 업데이트 날짜
- 지원하는 API 목록
- 특수 파라미터 및 기능
- 제한사항

### 3. Config 클래스 작성

`CompatibleConfig`를 상속하여 Provider별 설정 클래스를 작성합니다.

```csharp
public class NewProviderConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.newprovider.com/v1";

    // Provider 고유 설정 속성들...

    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = ApiKey ?? string.Empty,
            // DefaultHeaders = ... (필요 시)
        };
    }
}
```

### 4. Generator 클래스 작성

Provider가 사용하는 API 타입에 따라 기반 클래스를 선택합니다:

| API 타입 | 기반 클래스 |
|----------|------------|
| Chat Completions API | `OpenAIChatMessageGenerator` |
| Responses API | `OpenAIResponseMessageGenerator` |

```csharp
internal class NewProviderMessageGenerator : OpenAIChatMessageGenerator
{
    private readonly NewProviderConfig _config;

    public NewProviderMessageGenerator(NewProviderConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }
}
```

### 5. Extension Method 등록

`IHiveServiceBuilder`의 Provider 등록 확장 메서드를 추가합니다.

```csharp
public static IHiveServiceBuilder AddNewProvider(
    this IHiveServiceBuilder builder,
    string providerName,
    NewProviderConfig config)
{
    builder.AddMessageGenerator(providerName, new NewProviderMessageGenerator(config));
    return builder;
}
```

---
