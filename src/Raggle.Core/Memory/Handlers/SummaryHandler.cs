using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.ChatCompletion;
using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Abstractions.Memory;
using Raggle.Core.Extensions;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class SummaryHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileStorage _documentStorage;

    public SummaryHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IFileStorage>();        
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

        await foreach (var section in _documentStorage.GetDocumentJsonAsync<DocumentFragment>(
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

            await _documentStorage.UpsertDocumentJsonAsync(
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
        var request = new ChatCompletionRequest
        {
            Model = options.ModelName,
            System = GetSystemInstructionPrompt(),
        };
        request.Messages.Append<UserMessage>(new TextContent
        {
            Index = 0,
            Text = $"Summarize This:\n\n{information}",
        });

        var chat = _serviceProvider.GetRequiredKeyedService<IChatCompletionService>(options.ServiceKey);
        var response = await chat.GenerateMessageAsync(request, cancellationToken);

        var sb = new StringBuilder();
        foreach (var item in response.Content?.Content ?? [])
        {
            if (item is TextContent text)
            {
                sb.AppendLine(text.Text);
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
