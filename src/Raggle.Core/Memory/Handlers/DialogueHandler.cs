using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Json;
using Raggle.Abstractions.Memory;
using Raggle.Abstractions.Messages;
using Raggle.Core.Extensions;
using Raggle.Core.Memory.Document;
using System.Text;
using System.Text.RegularExpressions;

namespace Raggle.Core.Memory.Handlers;

public class DialogueHandler : IPipelineHandler
{
    private static readonly Regex DialogueRegex = new Regex(
        @"<qa>\s*<q>\s*(.*?)\s*</q>\s*<a>\s*(.*?)\s*</a>\s*</qa>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentStorage _documentStorage;

    public DialogueHandler(IServiceProvider service)
    {
        _serviceProvider = service;
        _documentStorage = service.GetRequiredService<IDocumentStorage>();
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
        var dialogues = new List<DocumentFragment>();

        await foreach (var section in _documentStorage.GetDocumentJsonAsync<DocumentFragment>(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            suffix: pipeline.GetPreviousStep() ?? "unknown",
            cancellationToken: cancellationToken))
        {

            var str = JsonObjectConverter.ConvertTo<string>(section.Content)
                ?? throw new InvalidOperationException("The document content is not a string.");
            
            var content = await GenerateDialoguesAsync(str, options, cancellationToken);
            var dialogue = new DocumentFragment
            {
                Index = section.Index,
                Unit = section.Unit,
                From = section.From,
                To = section.To,
                Content = content,
            };
            dialogues.Add(dialogue);
        }

        await _documentStorage.UpsertDocumentJsonAsync(
            collectionName: pipeline.CollectionName,
            documentId: pipeline.DocumentId,
            fileName: Path.GetFileNameWithoutExtension(pipeline.FileName),
            suffix: pipeline.CurrentStep ?? "unknown",
            values: dialogues,
            cancellationToken: cancellationToken);

        return pipeline;
    }

    #region Private Methods

    private async Task<IEnumerable<Tuple<string,string>>> GenerateDialoguesAsync(
        string information,
        Options options,
        CancellationToken cancellationToken)
    {
        var request = new ChatCompletionRequest
        {
            System = GetSystemInstruction(),
            Model = options.ModelName,
        };
        request.Messages.AddUserMessage(new TextContent
        {
            Index = 0,
            Text = $"Generate QA pairs In This:\n\n{information}" 
        });

        var chat = _serviceProvider.GetRequiredKeyedService<IChatCompletionService>(options.ServiceKey);
        var response = await chat.ChatCompletionAsync(request, cancellationToken);

        var sb = new StringBuilder();
        foreach (var item in response.Message?.Content ?? [])
        {
            if (item is TextContent text)
            {
                sb.AppendLine(text.Text);
            }
        }
        var answer = sb.ToString();
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("Failed to generate QA pairs.");
        }
        return ParseDialoguesFromText(answer);
    }

    private IEnumerable<Tuple<string, string>> ParseDialoguesFromText(string text)
    {
        var dialogues = new List<Tuple<string,string>>();
        var matches = DialogueRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 2)
            {
                var question = match.Groups[1].Value.Trim();
                var answer = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                {
                    dialogues.Add(new Tuple<string, string>(question,answer));
                }
            }
        }

        if (dialogues.Count == 0)
        {
            throw new FormatException("No QA pairs found within <qa> tags.");
        }

        return dialogues;
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

