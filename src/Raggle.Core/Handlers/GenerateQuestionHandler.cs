using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Core.Document;
using Raggle.Core.Utils;

namespace Raggle.Core.Handlers;

public class GenerateQuestionHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IChatCompletionService _chatEngine;

    public GenerateQuestionHandler(
        IDocumentStorage documentStorage,
        IChatCompletionService chatEngine)
    {
        _documentStorage = documentStorage;
        _chatEngine = chatEngine;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.RawText))
                continue;

            // 문서 청크에서 질문 생성
            // Tool 사용,
        }

        return pipeline;
    }

    #region

    private async Task<IEnumerable<DocumentChunk>> GetDocumentChunksAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filePaths = await _documentStorage.GetDocumentFilesAsync(
            pipeline.Document.CollectionName,
            pipeline.Document.DocumentId,
            cancellationToken);
        var chunkFilePaths = filePaths.Where(x => x.EndsWith(DocumentFileHelper.ChunkedFileExtension));

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

    private async Task<IEnumerable<string>> GenerateQuestionsAsync(string text, CancellationToken cancellationToken)
    {
        var response = await _chatEngine.ChatCompletionAsync(text, cancellationToken);
        return new List<string>();
    }

    #endregion
}
