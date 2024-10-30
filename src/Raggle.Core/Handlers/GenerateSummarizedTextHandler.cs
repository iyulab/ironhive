using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Memory.Document;
using Raggle.Abstractions.Messages;
using Raggle.Core.Document;
using Raggle.Core.Utils;
using System.Text;

namespace Raggle.Core.Handlers;

public class GenerateSummarizedTextHandler : IPipelineHandler
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IChatCompletionService _chatService;
    private readonly ChatCompletionOptions _chatOptions;

    public GenerateSummarizedTextHandler(
        IDocumentStorage documentStorage,
        IChatCompletionService chatService,
        ChatCompletionOptions chatOptions)
    {
        _documentStorage = documentStorage;
        _chatService = chatService;
        _chatOptions = chatOptions;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var chunks = await GetDocumentChunksAsync(pipeline, cancellationToken);

        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.RawText))
                throw new InvalidOperationException("No text content found in the document chunk.");

            var answer = await GenerateSummarizedTextAsync(chunk.RawText, cancellationToken);
            chunk.SummarizedText = answer;
            await UpdateDocumentChunkAsync(pipeline, chunk, cancellationToken);
        }

        return pipeline;
    }

    #region Private Methods

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

    private async Task UpdateDocumentChunkAsync(DataPipeline pipeline, DocumentChunk chunk, CancellationToken cancellationToken)
    {
        var filename = DocumentFileHelper.GetChunkedFileName(pipeline.Document.FileName, chunk.ChunkIndex);
        var chunkStream = JsonDocumentSerializer.SerializeToStream(chunk);
        await _documentStorage.WriteDocumentFileAsync(
            pipeline.Document.CollectionName,
            pipeline.Document.DocumentId,
            filename,
            chunkStream,
            overwrite: true,
            cancellationToken);
    }

    private async Task<string> GenerateSummarizedTextAsync(string text, CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddUserMessage(new TextContentBlock
        {
            Text = $"Summarize This:\n\n{text}",
        });
        _chatOptions.System = GetSystemInstructionPrompt();
        var response = await _chatService.ChatCompletionAsync(history, _chatOptions);
        if (response.State == ChatResponseState.Stop)
        {
            var textAnswer = new StringBuilder();
            foreach (var content in response.Contents)
            {
                if (content is TextContentBlock textContent)
                {
                    textAnswer.AppendLine(textContent.Text);
                }
            }
            var answer = textAnswer.ToString();
            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException("Failed to generate questions.");
            }
            return answer;
        }
        else
        {
            throw new InvalidOperationException("Failed to generate questions.");
        }
    }

    private static string? GetSystemInstructionPrompt()
    {
        return """
        You are an advanced language model designed to extract and present the most relevant and useful information from a given text. 
        Your task is to read the provided text and generate a concise, clear, and informative summary that highlights key points, important details, and actionable insights. 
        Ensure that the summary is well-structured, easy to understand, and free of unnecessary jargon.
        """;
    }

    #endregion
}
