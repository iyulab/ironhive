using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using System.Text.RegularExpressions;

namespace IronHive.Core.Handlers;

public class Dialogue
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class QnAExtractionHandler : IMemoryPipelineHandler
{
    private static readonly Regex DialogueRegex = new Regex(
        @"<qa>\s*<q>\s*(.*?)\s*</q>\s*<a>\s*(.*?)\s*</a>\s*</qa>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly IMessageGenerationService _service;

    public class Options
    {
        public required string Provider { get; set; }
        public required string Model { get; set; }
    }

    public QnAExtractionHandler(IMessageGenerationService chat)
    {
        _service = chat;
    }

    public async Task<MemoryPipelineContext> ProcessAsync(MemoryPipelineContext context, CancellationToken cancellationToken)
    {
        if (context.Payload.TryConvertTo<IEnumerable<string>>(out var chunks))
        {
            var options = context.Options.ConvertTo<Options>()
                ?? throw new InvalidOperationException($"Must provide options for {nameof(QnAExtractionHandler)}");

            var dialogues = new List<Dialogue>();
            foreach (var chunk in chunks)
            {
                var request = new MessageGenerationRequest
                {
                    Provider = options.Provider,
                    Model = options.Model,  
                    System = GetInstructions(),
                    Messages = [new UserMessage
                    {
                        Id = Guid.NewGuid().ToShort(),
                        Content = [ new TextMessageContent { Value = $"generate QnA pairs in this information:\n\n{chunk}" } ]
                    }],
                    Temperature = 0.0f,
                    TopP = 0.5f
                };
                var result = await _service.GenerateMessageAsync(request, cancellationToken);
                var text = result.Message?.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value
                    ?? throw new InvalidOperationException("No response from the chat completion service.");
                dialogues.AddRange(ParseFrom(text));
            }
            context.Payload = dialogues;

            return context;
        }
        else
        {
            throw new InvalidOperationException("The document content is not a string.");
        }
    }

    // 결과 텍스트를 정규식으로 파싱하여 QnA 쌍을 추출합니다.
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

    // QnA를 생성하기 위한 지침입니다.
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

