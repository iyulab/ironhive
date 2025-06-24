namespace IronHive.Abstractions.Embedding;

/// <summary>
/// 임베딩을 생성하는 기능을 제공하는 프로바이더 인터페이스입니다.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// 공급자의 이름을 가져옵니다.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 지정된 모델을 사용하여 단일 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="modelId">사용할 모델의 식별자입니다.</param>
    /// <param name="input">임베딩할 입력 문자열입니다.</param>
    /// <returns>임베딩 벡터를 반환합니다.</returns>
    Task<IEnumerable<float>> EmbedAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 모델을 사용하여 다수의 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="modelId">사용할 모델의 식별자입니다.</param>
    /// <param name="inputs">임베딩할 입력 문자열 목록입니다.</param>
    /// <returns>임베딩 결과들의 목록을 반환합니다.</returns>
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 모델을 사용하여 입력 문자열의 토큰 수를 계산합니다.
    /// </summary>
    Task<int> CountTokensAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default);
}
