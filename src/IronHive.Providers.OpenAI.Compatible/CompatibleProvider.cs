namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// OpenAI 호환 API를 제공하는 서비스 제공자를 정의합니다.
/// </summary>
public enum CompatibleProvider
{
    /// <summary>
    /// xAI (Grok) - Elon Musk의 AI 회사
    /// Base URL: https://api.x.ai/v1
    /// </summary>
    xAI,

    /// <summary>
    /// Groq - 초고속 LLM 추론 서비스
    /// Base URL: https://api.groq.com/openai/v1
    /// </summary>
    Groq,

    /// <summary>
    /// DeepSeek - 중국의 AI 연구 기업
    /// Base URL: https://api.deepseek.com/v1
    /// </summary>
    DeepSeek,

    /// <summary>
    /// Together AI - 오픈소스 모델 호스팅 플랫폼
    /// Base URL: https://api.together.xyz/v1
    /// </summary>
    TogetherAI,

    /// <summary>
    /// Fireworks AI - 고성능 모델 서빙 플랫폼
    /// Base URL: https://api.fireworks.ai/inference/v1
    /// </summary>
    Fireworks,

    /// <summary>
    /// Perplexity - 검색 기반 AI 서비스
    /// Base URL: https://api.perplexity.ai
    /// </summary>
    Perplexity,

    /// <summary>
    /// OpenRouter - 다중 AI 모델 라우터
    /// Base URL: https://openrouter.ai/api/v1
    /// </summary>
    OpenRouter,

    /// <summary>
    /// vLLM - 고성능 오픈소스 LLM 서빙 엔진 (Self-hosted)
    /// Base URL: http://localhost:8000/v1 (기본값)
    /// </summary>
    vLLM,

    /// <summary>
    /// GPUStack - Self-hosted GPU 클러스터 관리 플랫폼
    /// Base URL: 사용자 정의
    /// </summary>
    GPUStack,

    /// <summary>
    /// 사용자 정의 OpenAI 호환 서비스
    /// </summary>
    Custom,
}
