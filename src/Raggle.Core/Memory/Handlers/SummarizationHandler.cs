using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Extensions;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Messages;
using Raggle.Core.Memory.Document;
using System.Text;

namespace Raggle.Core.Memory.Handlers;

public class SummarizationHandler : IPipelineHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentManager _documentManager;

    public SummarizationHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
        _documentManager = service.GetRequiredService<IDocumentManager>();
    }

    public class Options
    {
        public string ServiceKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentMetadata<Options>()
            ?? throw new InvalidOperationException($"Must provide options for {pipeline.CurrentStep}.");

        var summaries = new List<DocumentSection>();

        await foreach (var section in _documentManager.GetDocumentFilesAsync<DocumentSection>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken))
        {
            var str = section.Content?.Get<string>()
                ?? throw new InvalidOperationException("The document content is not a string.");

            var content = await GenerateSummarizedTextAsync(str, options, cancellationToken);
            var summary = new DocumentSection
            {
                Index = section.Index,
                Unit = section.Unit,
                From = section.From,
                To = section.To,
                Content = content,
            };
            summaries.Add(summary);
        }

        await _documentManager.UpsertDocumentFilesAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
            suffix: pipeline.CurrentStep ?? "unknown",
            values: summaries,
            cancellationToken: cancellationToken);

        return pipeline;
    }

    #region Private Methods

    private async Task<string> GenerateSummarizedTextAsync(
        string text, 
        Options options, 
        CancellationToken cancellationToken)
    {
        var request = new ChatCompletionRequest
        {
            Model = options.ModelName,
            System = GetSystemInstructionPrompt(),
            Messages = new ChatHistory(),
        };
        request.Messages.AddUserMessage(new TextContentBlock
        {
            Text = $"Summarize This:\n\n{text}",
        });

        var chat = _serviceProvider.GetRequiredKeyedService<IChatCompletionService>(options.ServiceKey);
        var response = await chat.ChatCompletionAsync(request, cancellationToken);
        if (response.Completed)
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
