using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Pipelines;
using System.Text;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// TextChunkerHandler는 주어진 텍스트를 청크로 나누는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class TextChunkingPipeline : IPipeline<PipelineContext<string>, PipelineContext<IEnumerable<string>>>
{
    private readonly IEmbeddingService _embedder;

    public TextChunkingPipeline(IEmbeddingService embedder)
    {
        _embedder = embedder;
    }

    public int ChunkSize { get; init; } = 2048;

    /// <summary>
    /// 텍스트를 청크로 나누는 핸들러입니다.
    /// 
    /// TODO: 다시 작성
    /// 1. seperator를 여러가지 지원
    /// 2. 줄바꿈(\n) => 쉼표 또는 마침표(, .) => 공백 ( ) 순의 청크 방법론 고려
    /// 3. 속도를 위해 병렬 처리
    /// </summary>
    public async Task<PipelineContext<IEnumerable<string>>> InvokeAsync(
        PipelineContext<string> input, 
        CancellationToken cancellationToken = default)
    {
        var target = input.Target.ConvertTo<VectorMemoryTarget>()
            ?? throw new InvalidOperationException("target is not a VectorMemoryTarget");
        if (string.IsNullOrWhiteSpace(input.Payload))
            throw new InvalidOperationException("the document content is empty");

        var lines = input.Payload.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();

        long total = 0;
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var tokenCount = await _embedder.CountTokensAsync(
                target.EmbeddingProvider,
                target.EmbeddingModel,
                line,
                cancellationToken);

            if (tokenCount <= 0)
                continue; // Skip empty

            // 라인이 넘칠 경우
            if (tokenCount > ChunkSize)
            {
                if (sb.Length > 0)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                    total = 0;
                }

                var chars = line.Chunk(ChunkSize);
                foreach (var c in chars)
                {
                    chunks.Add(c.ToString()!);
                }
                continue;
            }

            if (total + tokenCount > ChunkSize)
            {
                chunks.Add(sb.ToString());
                sb.Clear();
                total = 0;
            }
            sb.AppendLine(line);
            total += tokenCount;
        }

        if (sb.Length > 0)
        {
            chunks.Add(sb.ToString());
        }

        if (chunks.Count == 0)
            throw new InvalidOperationException("the document content is empty");

        return new PipelineContext<IEnumerable<string>>
        {
            Source = input.Source,
            Target = input.Target,
            Payload = chunks
        };
    }
}
