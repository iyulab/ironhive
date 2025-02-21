using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class ChunkingHandler : IPipelineHandler
{
    private readonly IFileStorage _documentStorage;
    private readonly ITextTokenizer _textTokenizer;

    public ChunkingHandler(IServiceProvider service)
    {
        _documentStorage = service.GetRequiredService<IFileStorage>();

        // 서비스 마다 다른 토크나이저 사용해야함
        _textTokenizer = new TiktokenTokenizer();
    }

    public class Options
    {
        public int MaxTokens { get; set; } = 2048;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentOptions<Options>() ?? new Options();

        var section = await _documentStorage.GetDocumentJsonFirstAsync<DocumentFragment>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken);

        var content = section.Content.ConvertTo<IEnumerable<string>>()
            ?? throw new InvalidOperationException("The document content is not found");
        var chunks = new List<DocumentFragment>();

        long totalTokenCount = 0;
        int sectionIndex = 0;
        int sectionFrom = 1;
        int sectionTo = 0;
        var sb = new StringBuilder();

        foreach (var item in content)
        {
            sectionTo++;
            var tokenCount = _textTokenizer.Encode(item).Count;

            if (totalTokenCount + tokenCount > options.MaxTokens)
            {
                var chunk = new DocumentFragment
                {
                    Index = sectionIndex,
                    Unit = section?.Unit,
                    From = sectionFrom,
                    To = sectionTo,
                    Content = sb.ToString(),
                };
                chunks.Add(chunk);

                sectionIndex++;
                sb.Clear();
                totalTokenCount = 0;
                sectionFrom = sectionTo + 1;
            }

            sb.AppendLine(item);
            totalTokenCount += tokenCount;
        }

        if (sb.Length > 0)
        {
            var chunk = new DocumentFragment
            {
                Index = sectionIndex,
                Unit = section?.Unit,
                From = sectionFrom,
                To = sectionTo,
                Content = sb.ToString(),
            };

            chunks.Add(chunk);
        }

        await _documentStorage.UpsertDocumentJsonAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
            suffix: pipeline.CurrentStep ?? "unknown",
            values: chunks,
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
