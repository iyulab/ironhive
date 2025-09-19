using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using IronHive.Abstractions.Workflow;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// 주어진 청크에서 벡터 임베딩을 생성합니다.
/// </summary>
public class CreateVectorsPipeline : IMemoryPipeline
{
    private readonly IEmbeddingService _embedder;

    public CreateVectorsPipeline(IEmbeddingService embedder)
    {
        _embedder = embedder;
    }

    /// <inheritdoc />
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context, 
        CancellationToken cancellationToken = default)
    {
        if (context.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");
        if (!context.Payload.TryGetValue("chunks", out var chunks))
            throw new InvalidOperationException("chunks not found in context items");

        var points = new List<VectorRecord>();
        if (chunks is IEnumerable<DialogueExtractionPipeline.Dialogue> dialogues)
        {
            var embeddings = await _embedder.EmbedBatchAsync(
                    target.EmbeddingProvider,
                    target.EmbeddingModel,
                    dialogues.Select(x => x.Question),
                    cancellationToken);

            if (embeddings == null || embeddings.Count() != dialogues.Count())
                throw new InvalidOperationException("failed to get embeddings for dialogues");

            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = dialogues.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for dialogue");

                points.Add(new VectorRecord
                {
                    VectorId = Guid.NewGuid().ToString(),
                    SourceId = context.Source.Id,
                    Vectors = vector,
                    Payload = new Dictionary<string, object?>
                    {
                        { "question", content.Question },
                        { "answer", content.Answer },
                    }
                });
            }
        }
        else if (chunks is IEnumerable<string> texts)
        {
            var embeddings = await _embedder.EmbedBatchAsync(
               target.EmbeddingProvider,
               target.EmbeddingModel,
               texts,
               cancellationToken);

            if (embeddings == null || embeddings.Count() != texts.Count())
                throw new InvalidOperationException("failed to get embeddings for texts");

            for (var i = 0; i < embeddings.Count(); i++)
            {
                var content = texts.ElementAt(i);
                var vector = embeddings.ElementAt(i).Embedding;

                if (vector == null || content == null)
                    throw new InvalidOperationException("failed to get embedding for dialogue");

                points.Add(new VectorRecord
                {
                    VectorId = Guid.NewGuid().ToString(),
                    SourceId = context.Source.Id,
                    Vectors = vector,
                    Payload = new Dictionary<string, object?>
                    {
                        { "text", content },
                    }
                });
            }
        }
        else
        {
            return TaskStepResult.Fail(new InvalidOperationException("payload is not a IEnumerable<Dialogue> or IEnumerable<string>"));
        }

        context.Payload["vectors"] = points;
        return TaskStepResult.Success();
    }
}
