using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Workflow;
using System.Text;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// TextChunkerHandler는 주어진 텍스트를 청크로 나누는 메모리 파이프라인 핸들러입니다.
/// </summary>
public class TextChunkingPipeline : IMemoryPipeline<TextChunkingPipeline.Options>
{
    private readonly IEmbeddingService _embedder;

    public TextChunkingPipeline(IEmbeddingService embedder)
    {
        _embedder = embedder;
    }

    public record Options(int ChunkSize = 2048);

    /// <summary>
    /// 텍스트를 청크로 나누는 핸들러입니다.
    /// 
    /// TODO: 다시 작성
    /// 1. seperator를 여러가지 지원
    /// 2. 줄바꿈(\n) => 쉼표 또는 마침표(, .) => 공백 ( ) 순의 청크 방법론 고려
    /// 3. 속도를 위해 병렬 처리
    /// </summary>
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context, 
        Options options, 
        CancellationToken cancellationToken = default)
    {
        if (context.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");
        if (!context.Payload.TryGetValue<string>("text", out var text))
            throw new InvalidOperationException("payload is not a string");

        var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (tokenCount > options.ChunkSize)
            {
                if (sb.Length > 0)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                    total = 0;
                }

                var chars = line.Chunk(options.ChunkSize);
                foreach (var c in chars)
                {
                    chunks.Add(c.ToString()!);
                }
                continue;
            }

            if (total + tokenCount > options.ChunkSize)
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

        context.Payload.Add("chunks", chunks);
        return TaskStepResult.Success();
    }
}
