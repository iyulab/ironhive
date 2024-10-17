using Raggle.Abstractions.Memory;

namespace Raggle.Core.Handlers;

public class UploadDocumentHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;

    public UploadDocumentHandler(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        await _documentStorage.WriteDocumentFileAsync(
            collection: "",
            documentId: "",
            filePath: "",
            new MemoryStream(),
            cancellationToken: cancellationToken);

        return pipeline;
    }
}
