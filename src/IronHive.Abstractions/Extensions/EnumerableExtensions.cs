using IronHive.Abstractions.Embedding;

namespace System.Collections.Generic;

public static class EnumerableExtensions
{
    /// <summary>
    /// 주어진 토큰 제한에 따라 <see cref="EmbeddingTokens"/>를 여러 개의 청크로 나눕니다.
    /// </summary>
    /// <param name="source">나눌 원본 배열입니다.</param>
    /// <param name="maxTokens">각 배열이 가질 수 있는 최대 토큰 수입니다.</param>
    /// <returns>토큰 제한에 따라 나누어진 <see cref="EmbeddingTokens"/> 배열의 컬렉션입니다.</returns>
    public static IEnumerable<IEnumerable<EmbeddingTokens>> ChunkBy(this IEnumerable<EmbeddingTokens> source, int maxTokens)
    {
        if (maxTokens < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "maxTokens must be greater than zero.");

        var result = new List<IEnumerable<EmbeddingTokens>>();
        var currentChunk = new List<EmbeddingTokens>();
        var currentTokenSum = 0;

        foreach (var item in source)
        {
            // 현재 청크에 아이템을 추가했을 때 최대 토큰 수를 초과하는지 확인합니다.
            if (currentChunk.Count > 0 && currentTokenSum + item.TokenCount > maxTokens)
            {
                result.Add(currentChunk);
                currentChunk = new List<EmbeddingTokens>(); // 새 청크 시작
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
}
