namespace IronHive.Abstractions.Embedding;

/// <summary>
/// 다양한 임베딩 프로바이더를 통합하여 임베딩을 생성하는 서비스 인터페이스입니다.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// 임베딩 생성기를 관리하는 컬렉션입니다.
    /// </summary>
    public IKeyedCollection<IEmbeddingGenerator> Generators { get; }

    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 단일 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="provider">사용할 공급자의 이름입니다.</param>
    /// <param name="modelId">사용할 모델의 이름 또는 식별자입니다.</param>
    /// <param name="input">임베딩할 입력 문자열입니다.</param>
    /// <returns>임베딩 벡터를 반환합니다.</returns>
    Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 다수의 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="provider">사용할 공급자의 이름입니다.</param>
    /// <param name="modelId">사용할 모델의 이름 또는 식별자입니다.</param>
    /// <param name="inputs">임베딩할 입력 문자열 목록입니다.</param>
    /// <returns>임베딩 결과들의 목록을 반환합니다.</returns>
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 입력 문자열의 토큰 수를 계산합니다.
    /// </summary>
    Task<int> CountTokensAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 다수의 입력 문자열의 토큰 수를 계산합니다.
    /// </summary>
    Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);
}
