using System.Text;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Handlers;

public class TextChunkerHandler : IPipelineHandler
{
    private readonly IEmbeddingGenerationService _service;

    public TextChunkerHandler(IEmbeddingGenerationService service)
    {
        _service = service;
    }

    public class Options
    {
        public int ChunkSize { get; set; } = 2048;
    }

    /// <summary>
    /// 텍스트를 청크로 나누는 핸들러입니다.
    /// 
    /// TODO: 다시 작성
    /// 1. seperator를 여러가지 지원
    /// 2. 줄바꿈(\n) => 쉼표 또는 마침표(, .) => 공백 ( ) 순의 청크 방법론 고려
    /// 3. 속도를 위해 병렬 처리
    /// </summary>
    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var target = context.Target.ConvertTo<VectorMemoryTarget>()
            ?? throw new InvalidOperationException("target is not a VectorMemoryTarget");

        if (context.Payload.TryConvertTo<string>(out var content))
        {
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var options = context.Options.ConvertTo<Options>() ?? new Options();
            var chunks = new List<string>();

            long total = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var tokenCount = await _service.CountTokensAsync(
                    provider: target.EmbedProvider, 
                    modelId: target.EmbedModel,
                    input: line, 
                    cancellationToken: cancellationToken);

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

            context.Payload = chunks;
            return context;
        }
        else
        {
            throw new InvalidOperationException("The document content is not a string.");
        }
    }
}
