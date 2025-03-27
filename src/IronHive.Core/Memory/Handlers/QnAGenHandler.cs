using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using System.Text.RegularExpressions;

namespace IronHive.Core.Memory.Handlers;

public class Dialogue
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class QnAGenHandler : IPipelineHandler
{
    private static readonly Regex DialogueRegex = new Regex(
        @"<qa>\s*<q>\s*(.*?)\s*</q>\s*<a>\s*(.*?)\s*</a>\s*</qa>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IChatCompletionService _service;

    public class Options
    {
        public required string Provider { get; set; }
        public required string Model { get; set; }
    }

    public QnAGenHandler(IChatCompletionService chat)
    {
        _service = chat;
    }

    public async Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Payload.TryConvertTo<IEnumerable<string>>(out var chunks))
        {
            var options = pipeline.GetCurrentOptions<Options>()
                ?? throw new InvalidOperationException($"Must provide options for {pipeline.CurrentStep}.");

            var serviceOptions = new ChatCompletionOptions
            {
                Provider = options.Provider,
                Model = options.Model, 
                Instructions = GetInstructions(),
                Temperature = 0.0f,
                TopK = 0,
                TopP = 0.9f
            };

            var dialogues = new List<Dialogue>();
            foreach (var chunk in chunks)
            {
                var message = new UserMessage();
                message.Content.AddText($"generate QnA pairs in this information:\n\n{chunk}");
                var result = await _service.GenerateMessageAsync(message, serviceOptions, cancellationToken);
                var text = result.Content.OfType<AssistantTextContent>().FirstOrDefault()?.Value
                    ?? throw new InvalidOperationException("No response from the chat completion service.");
                dialogues.AddRange(ParseFrom(text));
            }
            pipeline.Payload = dialogues;

            return pipeline;
        }
        else
        {
            throw new InvalidOperationException("The document content is not a string.");
        }
    }

    private static List<Dialogue> ParseFrom(string text)
    {
        var dialogues = new List<Dialogue>();
        var matches = DialogueRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 2)
            {
                var question = match.Groups[1].Value.Trim();
                var answer = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                {
                    dialogues.Add(new Dialogue
                    {
                        Question = question,
                        Answer = answer
                    });
                }
            }
        }

        if (dialogues.Count == 0)
        {
            throw new FormatException("No QA pairs found within <qa> tags.");
        }

        return dialogues;
    }

    private static string GetInstructions()
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
}

