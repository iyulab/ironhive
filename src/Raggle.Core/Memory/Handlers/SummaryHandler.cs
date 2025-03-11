using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class SummaryHandler : IPipelineHandler
{
    private readonly IChatCompletionService _chat;
    private readonly IFileStorage _file;

    public SummaryHandler(IChatCompletionService chat, IFileStorage file)
    {
        _chat = chat;
        _file = file;
    }

    public class Options
    {
        public string ServiceKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentOptions<Options>()
            ?? throw new InvalidOperationException($"Must provide options for {pipeline.CurrentStep}.");

        await foreach (var section in _file.GetDocumentJsonAsync<DocumentFragment>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken))
        {
            var str = section.Content.ConvertTo<string>()
                ?? throw new InvalidOperationException("The document content is not a string.");

            var content = await GenerateSummarizedTextAsync(str, options, cancellationToken);
            var summary = new DocumentFragment
            {
                Index = section.Index,
                Unit = section.Unit,
                From = section.From,
                To = section.To,
                Content = content,
            };

            await _file.UpsertDocumentJsonAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
                suffix: pipeline.CurrentStep ?? "unknown",
                value: summary,
                index: section.Index,
                cancellationToken: cancellationToken);
        }
        return pipeline;
    }

    #region Private Methods

    private async Task<string> GenerateSummarizedTextAsync(
        string information, 
        Options options, 
        CancellationToken cancellationToken)
    {
        var context = new MessageContext([]);
        context.Messages.AddUserMessage(new TextContent
        {
            Value = $"Summarize This:\n\n{information}",
        });
        var response = await _chat.InvokeAsync(context, new ChatCompletionOptions
        {
            Model = options.ModelName,
            System = GetSystemInstructionPrompt(),
        }, cancellationToken);

        var sb = new StringBuilder();
        foreach (var item in response.Data?.Content ?? [])
        {
            if (item is TextContent text)
            {
                sb.AppendLine(text.Value);
            }
        }
        var answer = sb.ToString();
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("Failed to generate questions.");
        }
        return answer;
    }

    private static string GetSystemInstructionPrompt()
    {
        return """
        You are an AI designed to accurately summarize text without adding or inferring information.

        [SUMMARIZATION RULES]
        - Summarize only the provided text without adding or inferring information.
        - Use short, clear, complete sentences.
        - Eliminate redundancy and repetition.
        - Do not include these phrases:
            - This article
            - This document
            - This page

        [EXAMPLES]
        Original: "Hello, how are you?"
        Summary: "Hello."

        Original: "The quick brown fox jumps over the lazy dog."
        Summary: "A fox jumps over a dog."
        [END EXAMPLES]
        """;
    }

    #endregion
}
