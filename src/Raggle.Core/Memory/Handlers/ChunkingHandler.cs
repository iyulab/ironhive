using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using Raggle.Core.Tokenizers;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class ChunkingHandlerOptions
{
    public int MaxTokensPerChunk { get; set; } = 1024;
}

public class ChunkingHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly ITextTokenizer _textTokenizer;

    public ChunkingHandler(IServiceProvider service)
    {
        _documentStorage = service.GetRequiredService<IDocumentStorage>();

        // 서비스 마다 다른 토크나이저 사용해야함
        _textTokenizer = new TiktokenTokenizer();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentMetadata<ChunkingHandlerOptions>()
            ?? new ChunkingHandlerOptions();

        var docFIle = await _documentStorage.ReadJsonDocumentFileAsync<DocumentSource>(
            pipeline.CollectionName,
            pipeline.DocumentId,
            pipeline.GetPreviousStepFileName(),
            cancellationToken: cancellationToken);

        int chunkIndex = 0;
        long totalTokenCount = 0;
        var sectionFrom = docFIle.Section?.From ?? 1;
        var sectionTo = sectionFrom;

        var sb = new StringBuilder();

        var contents = docFIle.Content as IEnumerable<string>
            ?? throw new InvalidOperationException("The document content is not a list of strings.");
        foreach (var content in contents)
        {
            var tokenCount = _textTokenizer.Encode(content).Count;

            if (totalTokenCount + tokenCount > options.MaxTokensPerChunk)
            {
                var chunk = new DocumentSource
                {
                    Source = docFIle.Source,
                    Section = new DocumentSegment
                    {
                        Unit = docFIle.Section?.Unit,
                        From = sectionFrom,
                        To = sectionTo,
                    },
                    Index = chunkIndex,
                    Content = sb.ToString(),
                };

                var filename = pipeline.GetCurrentStepFileName(chunkIndex);
                await _documentStorage.WriteJsonDocumentFileAsync(
                    collectionName: pipeline.CollectionName,
                    documentId: pipeline.DocumentId,
                    filePath: filename,
                    model: chunk,
                    cancellationToken: cancellationToken);

                chunkIndex++;
                sb.Clear();
                totalTokenCount = 0;
                sectionFrom = sectionTo + 1;
            }

            sb.Append(content);
            totalTokenCount += tokenCount;
            sectionTo++;
        }

        if (sb.Length > 0)
        {
            var chunk = new ChunkedFile
            {
                Source = extractedFile.Source,
                Section = new DocumentSegment
                {
                    Unit = extractedFile.Section?.Unit,
                    From = sectionFrom,
                    To = sectionTo,
                },
                Content = sb.ToString(),
            };

            var filename = pipeline.GetCurrentStepFileName(chunkIndex);
            await _documentStorage.WriteJsonDocumentFileAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                filePath: filename,
                model: chunk,
                cancellationToken: cancellationToken);
        }

        return pipeline;
    }
}
