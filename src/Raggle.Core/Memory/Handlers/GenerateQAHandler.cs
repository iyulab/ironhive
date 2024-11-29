using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Extensions;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Messages;
using Raggle.Core.Memory.Document;
using Raggle.Core.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace Raggle.Core.Memory.Handlers;

public class GenerateQAHandlerOptions
{
    public string ServiceKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
}

public class GenerateQAHandler : IPipelineHandler
{
    private static readonly Regex QAPairRegex = new Regex(
        @"<qa>\s*<q>\s*(.*?)\s*</q>\s*<a>\s*(.*?)\s*</a>\s*</qa>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;

    public GenerateQAHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        var options = pipeline.GetCurrentMetadata<GenerateQAHandlerOptions>()
            ?? throw new InvalidOperationException("No options found for GenerateQAPairsHandler.");
        var collectionName = pipeline.CollectionName;
        var documentId = pipeline.DocumentId;

        await foreach (var file in _documentStorage.GetDocumentFilesAsync(collectionName, documentId, cancellationToken))
        {
            if (!file.EndsWith(DocumentFileHelper.ChunkedFileExtension))
                continue;

            var steam = await _documentStorage.ReadDocumentFileAsync(collectionName, documentId, file, cancellationToken);
            var chunk = JsonDocumentSerializer.Deserialize<ChunkedDocument>(steam);

            string information;
            if (!string.IsNullOrWhiteSpace(chunk.SummarizedText))
                information = chunk.SummarizedText;
            else if (!string.IsNullOrWhiteSpace(chunk.RawText))
                information = chunk.RawText;
            else
                throw new InvalidOperationException("No text content found in the document chunk.");

            var qaPairs = await GenerateQAPairsAsync(information, options, cancellationToken);
            chunk.ExtractedQAPairs = qaPairs.Select((qa, index) => new QAPair
            {
                Index = index,
                Question = qa.Question,
                Answer = qa.Answer
            }).ToList();

            var fileName = DocumentFileHelper.GetChunkedFileName(pipeline.FileInfo.FileName, chunk.Index);
            var chunkStream = JsonDocumentSerializer.SerializeToStream(chunk);
            await _documentStorage.WriteDocumentFileAsync(
                collectionName: pipeline.CollectionName,
                documentId: pipeline.DocumentId,
                filePath: fileName,
                content: chunkStream,
                overwrite: true,
                cancellationToken: cancellationToken);
        }

        return pipeline;
    }

    #region Private Methods

    private async Task<IEnumerable<(string Question, string Answer)>> GenerateQAPairsAsync(
        string text,
        GenerateQAHandlerOptions options,
        CancellationToken cancellationToken)
    {
        var messages = new ChatHistory();
        messages.AddUserMessage(new TextContentBlock
        {
            Text = $"Generate QA pairs In This:\n\n{text}",
        });
        var request = new ChatCompletionRequest
        {
            System = GetSystemInstruction(),
            Model = options.ModelName,
            Messages = messages,
        };
        var chat = _serviceProvider.GetRequiredKeyedService<IChatCompletionService>(options.ServiceKey);
        var response = await chat.ChatCompletionAsync(request);

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
                throw new InvalidOperationException("Failed to generate QA pairs.");
            }
            return ParseQAPairsFromText(answer);
        }
        else
        {
            throw new InvalidOperationException("Failed to generate QA pairs.");
        }
    }

    private IEnumerable<(string Question, string Answer)> ParseQAPairsFromText(string text)
    {
        var qaPairs = new List<(string Question, string Answer)>();
        var matches = QAPairRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 2)
            {
                var question = match.Groups[1].Value.Trim();
                var answer = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                {
                    qaPairs.Add((question, answer));
                }
            }
        }

        if (!qaPairs.Any())
        {
            throw new FormatException("No QA pairs found within <qa> tags.");
        }

        return qaPairs;
    }

    private string GetSystemInstruction()
    {
        return """
        You are an AI that generates QA pairs strictly from the provided text without adding or inferring information.

        [QA RULES]
        - Use only the provided text to create questions and answers.
        - Enclose each QA pair within <qa> tags.
        - Use <q> and <a> tags inside <qa>.
        - Keep questions and answers clear and concise.
        - Do not add or infer any information not in the text.

        [EXAMPLE]
        Informaion: 
        On July 20, 1969, Apollo 11 successfully landed the first humans on the Moon, marking a significant achievement in space exploration.
        
        Response:
        <qa><q>When did Apollo 11 land on the Moon?</q><a>Apollo 11 landed on the Moon on July 20, 1969.</a></qa>
        <qa><q>What was the significance of Apollo 11's Moon landing?</q><a>The Moon landing marked the first time humans set foot on another celestial body, representing a major milestone in space exploration.</a></qa>
        <qa><q>Who were the first humans to walk on the Moon?</q><a>The first humans to walk on the Moon were Neil Armstrong and Buzz Aldrin.</a></qa>
        [END EXAMPLE]
        """;
    }

    #endregion
}

