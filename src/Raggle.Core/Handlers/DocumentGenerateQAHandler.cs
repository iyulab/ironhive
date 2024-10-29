using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Handlers;

public class DocumentGenerateQAHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IChatEngine _chatEngine;

    public DocumentGenerateQAHandler(
        IDocumentStorage documentStorage,
        IChatEngine chatEngine)
    {
        _documentStorage = documentStorage;
        _chatEngine = chatEngine;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.Text))
            {
                continue;
            }

            // 문서 청크에서 질문 생성
            // Tool 사용,
        }

        return pipeline;
    }

    private async Task<IEnumerable<DocumentChunk>> GetDocumentChunksAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            pipeline.Document.CollectionName,
            pipeline.Document.DocumentId,
            cancellationToken);
        var chunkFilePaths = filePaths.Where(x => x.EndsWith(DocumentFiles.ChunkedFileExtension));

        var chunks = new List<DocumentChunk>();
        foreach (var chunkFilePath in chunkFilePaths)
        {
            var chunkStream = await _documentStorage.ReadDocumentFileAsync(
                pipeline.Document.CollectionName,
                pipeline.Document.DocumentId,
                chunkFilePath,
                cancellationToken);

            var chunk = JsonDocumentSerializer.Deserialize<DocumentChunk>(chunkStream);
            chunks.Add(chunk);
        }
        return chunks;
    }
}
