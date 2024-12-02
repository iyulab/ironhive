using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Memory.Handlers;

public class EmbeddingsHandlerOptions
{
    public string ServiceKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}

public class EmbeddingsHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly IVectorStorage _vectorStorage;

    public EmbeddingsHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _vectorStorage = service.GetRequiredService<IVectorStorage>();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentMetadata<EmbeddingsHandlerOptions>()
            ?? throw new InvalidOperationException("No options found for embeddings handler.");
        var embedder = _serviceProvider.GetRequiredKeyedService<IEmbeddingService>(options.ServiceKey);
        var collectionName = pipeline.CollectionName;
        var documentId = pipeline.DocumentId;
        
        var points = new List<VectorPoint>();
        await foreach (var file in _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken))
        {
            if (!pipeline.IsPreviousStepFileName(file))
                continue;

            var docFile = await _documentStorage.ReadJsonDocumentFileAsync<DocumentSource>(
                collectionName: collectionName,
                documentId: documentId,
                filePath: file,
                cancellationToken: cancellationToken);

            if (docFile is ChunkedFile chunkedFile)
            {
                var response = await embedder.EmbeddingAsync(options.ModelName, chunkedFile.Content, cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.DocumentId,
                    Tags = pipeline.Tags?.ToArray(),
                    Payload = chunkedFile
                });
            }
            else if (docFile is SummarizedFile summarizedFile)
            {
                var response = await embedder.EmbeddingAsync(options.ModelName, summarizedFile.Summary, cancellationToken);
                points.Add(new VectorPoint
                {
                    VectorId = Guid.NewGuid(),
                    Vectors = response.Embedding,
                    DocumentId = pipeline.DocumentId,
                    Tags = pipeline.Tags?.ToArray(),
                    Payload = summarizedFile
                });
            }
            else if (docFile is DialogueFile dialogueFile)
            {
                var questions = dialogueFile.Dialogues.Select(x => x.Question).ToArray();
                var request = new EmbeddingRequest
                {
                    Model = options.ModelName,
                    Input = questions
                };
                var response = await embedder.EmbeddingsAsync(request, cancellationToken);
                for (var i = 0; i < response.Count(); i++)
                {
                    points.Add(new VectorPoint
                    {
                        VectorId = Guid.NewGuid(),
                        Vectors = response.ElementAt(i).Embedding,
                        DocumentId = pipeline.DocumentId,
                        Tags = pipeline.Tags?.ToArray(),
                        Payload = dialogueFile.Dialogues.ElementAt(i)
                    });
                }
            }
            else
            {
                throw new InvalidOperationException($"No text content found in the document file {file}");
            }
        }

        await _vectorStorage.UpsertVectorsAsync(pipeline.CollectionName, points, cancellationToken);
        return pipeline;
    }
}
