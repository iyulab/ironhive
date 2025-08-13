using System.Collections;

namespace IronHive.Abstractions.Embedding;

public static class EmbeddingGeneratorExtensions
{
    /// <summary>
    /// 입력 문자열들을 토큰화하여 임베딩 처리를 위한 단일 배치(Batch)를 비동기적으로 생성합니다.
    /// </summary>
    /// <param name="modelId">사용할 모델의 ID입니다.</param>
    /// <param name="inputs">임베딩을 생성할 원본 문자열의 열거형입니다.</param>
    /// <returns>토큰화된 입력 정보가 포함된 <see cref="EmbeddingInputBatch"/>를 반환합니다.</returns>
    public static async Task<EmbeddingInputBatch> CreateEmbeddingBatchAsync(
        this IEmbeddingGenerator generator,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        // IEnumerable에 대한 여러 번의 순회를 방지하고 인덱스 접근 성능을 보장하기 위해 리스트로 변환합니다.
        var inputList = inputs.ToList();
        var items = new EmbeddingInput[inputList.Count];

        await Parallel.ForEachAsync(Enumerable.Range(0, inputList.Count), cancellationToken, async (idx, ct) =>
        {
            var text = inputList[idx];
            var tokens = await generator.CountTokensAsync(modelId, text, ct);
            items[idx] = new EmbeddingInput(idx, text, tokens);
        });

        // 생성자에서 인덱스 순으로 정렬을 보장합니다.
        return [.. items];
    }
}

/// <summary>
/// 임베딩 입력 정보를 나타내는 레코드입니다.
/// 입력 문자열의 순서와 토큰 수를 포함합니다.
/// </summary>
public sealed record EmbeddingInput(int Index, string Text, int TokenCount);

/// <summary>
/// 임베딩 입력 데이터의 묶음(Batch)을 관리하는 클래스입니다.
/// 토큰 제한에 따라 여러 개의 작은 배치로 나눌 수 있는 기능을 제공합니다.
/// </summary>
public sealed class EmbeddingInputBatch : ICollection<EmbeddingInput>
{
    private readonly List<EmbeddingInput> _items;

    public EmbeddingInputBatch()
    {
        _items = [];
    }

    public EmbeddingInputBatch(IEnumerable<EmbeddingInput> items)
    {
        _items = items.OrderBy(i => i.Index).ToList();
    }

    /// <summary>
    /// 총 토큰 수를 반환합니다.
    /// </summary>
    public int TotalTokenCount => _items.Sum(i => i.TokenCount);

    /// <summary>
    /// 현재 배치를 최대 토큰 수를 기준으로 여러 개의 작은 배치(청크)로 나눕니다.
    /// </summary>
    /// <param name="maxTokensPerChunk">하나의 청크가 가질 수 있는 최대 토큰 수입니다.</param>
    /// <returns>나누어진 <see cref="EmbeddingInputBatch"/>의 열거형을 반환합니다.</returns>
    public IEnumerable<EmbeddingInputBatch> ChunkByTokens(int maxTokensPerChunk)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTokensPerChunk);

        var result = new List<EmbeddingInputBatch>();
        var currentChunk = new EmbeddingInputBatch();
        var currentTokenSum = 0;

        foreach (var item in _items)
        {
            // 현재 청크에 아이템을 추가했을 때 최대 토큰 수를 초과하는지 확인합니다.
            if (currentChunk.Count > 0 && currentTokenSum + item.TokenCount > maxTokensPerChunk)
            {
                result.Add(currentChunk);
                currentChunk = new EmbeddingInputBatch(); // 새 청크 시작
                currentTokenSum = 0;
            }

            currentChunk.Add(item);
            currentTokenSum += item.TokenCount;
        }

        // 마지막으로 처리된 청크가 비어있지 않다면 결과에 추가합니다.
        if (currentChunk.Count > 0)
        {
            result.Add(currentChunk);
        }

        return result;
    }

    public int Count => _items.Count;
    public bool IsReadOnly => false;
    public void Add(EmbeddingInput item) => _items.Add(item);
    public bool Remove(EmbeddingInput item) => _items.Remove(item);
    public void Clear() => _items.Clear();
    public bool Contains(EmbeddingInput item) => _items.Contains(item);
    public void CopyTo(EmbeddingInput[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public IEnumerator<EmbeddingInput> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}