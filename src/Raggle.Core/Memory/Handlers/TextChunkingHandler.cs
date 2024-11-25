using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory.Document;
using Raggle.Core.Tokenizers;
using Raggle.Core.Utils;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class TextChunkingHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly ITextTokenizer _textTokenizer;
    private readonly int _maxTokensPerChunk;

    public TextChunkingHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _textTokenizer = new TiktokenTokenizer();
        _maxTokensPerChunk = 1024;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var decodedDocument = await GetDecodedDocumentAsync(pipeline, cancellationToken);

        int chunkIndex = 0;
        foreach (var section in decodedDocument.Sections)
        {
            // 라인 단위로 텍스트 분할
            var lines = section.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunkText = new StringBuilder();
            var currentTokenCount = 0;

            // 각 라인을 사이즈 제한에 맞게 청크로 나누어 저장
            foreach (var line in lines)
            {
                var lineTokens = _textTokenizer.Encode(line);
                var lineTokenCount = lineTokens.Count;

                // 현재 청크에 라인을 추가했을 때 토큰 수가 초과하지 않는지 확인
                if (currentTokenCount + lineTokenCount > _maxTokensPerChunk)
                {
                    if (currentChunkText.Length > 0)
                    {
                        // 현재 청크 저장
                        var chunk = new ChunkedDocument
                        {
                            Index = chunkIndex++,
                            SourceFileName = pipeline.Document.FileName,
                            SourceSection = section.Identifier,
                            RawText = currentChunkText.ToString().Trim()
                        };

                        await UpsertChunkDocumentAsync(pipeline, chunk, cancellationToken);

                        // 새로운 청크 시작
                        currentChunkText.Clear();
                        currentTokenCount = 0;
                    }
                }

                // 현재 라인을 청크에 추가
                currentChunkText.AppendLine(line);
                currentTokenCount += lineTokenCount;
            }

            // 마지막 청크 저장 (남아있는 텍스트가 있을 경우)
            if (currentChunkText.Length > 0)
            {
                var finalChunk = new ChunkedDocument
                {
                    Index = chunkIndex++,
                    SourceFileName = pipeline.Document.FileName,
                    SourceSection = section.Identifier,
                    RawText = currentChunkText.ToString().Trim()
                };

                await UpsertChunkDocumentAsync(pipeline, finalChunk, cancellationToken);
            }
        }

        return pipeline;
    }

    #region Private Methods

    private async Task<DecodedDocument> GetDecodedDocumentAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var filename = DocumentFileHelper.GetParsedFileName(pipeline.Document.FileName);
        var fileStream = await _documentStorage.ReadDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: filename,
            cancellationToken: cancellationToken);

        var decodedDocument = JsonDocumentSerializer.Deserialize<DecodedDocument>(fileStream);
        return decodedDocument;
    }

    private async Task UpsertChunkDocumentAsync(
        DataPipeline pipeline,
        ChunkedDocument chunk,
        CancellationToken cancellationToken)
    {
        var filename = DocumentFileHelper.GetChunkedFileName(pipeline.Document.FileName, chunk.Index);
        var fileStream = JsonDocumentSerializer.SerializeToStream(chunk);
        await _documentStorage.WriteDocumentFileAsync(
            collectionName: pipeline.Document.CollectionName,
            documentId: pipeline.Document.DocumentId,
            filePath: filename,
            content: fileStream,
            cancellationToken: cancellationToken);
    }

    #endregion
}
