using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Embedding;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;

namespace Raggle.Core.Memory.Handlers;

public class EmbeddingsHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;

    public EmbeddingsHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IFileStorage>();
        _vectorStorage = service.GetRequiredService<IVectorStorage>();
    }

    public class Options
    {
        public string Model { get; set; } = string.Empty;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentOptions<Options>()
            ?? throw new InvalidOperationException("must provide options for embeddings handler");
        var embedder = _serviceProvider.GetRequiredService<IEmbeddingService>();
        
        var points = new List<VectorPoint>();

        await foreach (var section in _documentStorage.GetDocumentJsonAsync<DocumentFragment>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken))
        {
            if (section.Content == null)
                throw new InvalidOperationException($"No content found in the document");
            
            if (section.Content.TryConvertTo<string>(out var str))
            {
                var embedding = await embedder.EmbedAsync(options.Model, str, cancellationToken);
                if (embedding == null)
                    throw new InvalidOperationException($"failed to get embedding for {str}");

                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = embedding,
                    DocumentId = pipeline.DocumentId,
                    Tags = pipeline.Tags?.ToArray(),
                    Payload = section
                });
            }
            else if (section.Content.TryConvertTo<IEnumerable<DialogueHandler.Dialogue>>(out var dialogues))
            {
                var questions = dialogues.Select(x => x.Question).ToArray();
                var request = new EmbeddingsRequest
                {
                    Model = options.Model,
                    Input = questions
                };

                var response = await embedder.EmbedBatchAsync(request, cancellationToken);
                if (response.Embeddings == null && response.Embeddings?.Count() != questions.Length)
                    throw new InvalidOperationException($"failed to get embeddings for {questions}");

                for (var i = 0; i < response.Embeddings?.Count(); i++)
                {
                    var content = dialogues.ElementAt(i);
                    var embedding = response.Embeddings.FirstOrDefault(d => d.Index == i)?.Embedding
                        ?? throw new InvalidOperationException($"failed to get embedding for {questions[i]}");

                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = embedding,
                        DocumentId = pipeline.DocumentId,
                        Tags = pipeline.Tags?.ToArray(),
                        Payload = new DocumentFragment
                        {
                            Unit = section.Unit,
                            Index = section.Index,
                            From = section.From,
                            To = section.To,
                            Content = content
                        }
                    });
                }
            }
            else
            {
                throw new InvalidOperationException($"No content found in the document");
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.CollectionName, points, cancellationToken);
        return pipeline;
    }
}
