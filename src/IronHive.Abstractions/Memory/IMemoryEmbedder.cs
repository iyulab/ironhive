using IronHive.Abstractions.Embedding;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// MemoryService에 사용되는 임베딩 생성 인터페이스입니다.
/// </summary>
public interface IMemoryEmbedder
{
    /// <summary>
    /// 임베딩 생성 서비스의 공급자 키입니다.
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// 임베딩 생성 서비스의 모델 ID입니다.
    /// </summary>
    public string Model { get; }

    /// <summary>
    /// 단일 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="input">임베딩할 입력 문자열입니다.</param>
    /// <returns>임베딩 벡터를 반환합니다.</returns>
    Task<IEnumerable<float>> EmbedAsync(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 다수의 입력에 대한 임베딩을 생성합니다.
    /// </summary>
    /// <param name="inputs">임베딩할 입력 문자열 목록입니다.</param>
    /// <returns>임베딩 결과들의 목록을 반환합니다.</returns>
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 입력 문자열의 토큰 수를 계산합니다.
    /// </summary>
    Task<int> CountTokensAsync(
        string input,
        CancellationToken cancellationToken = default);
}
