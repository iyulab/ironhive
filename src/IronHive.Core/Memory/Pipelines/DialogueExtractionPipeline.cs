using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Workflow;
using System.Text.RegularExpressions;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// 주어진 텍스트에서 Q_A 쌍을 추출하는 핸들러입니다.
/// </summary>
public partial class DialogueExtractionPipeline : IMemoryPipeline<DialogueExtractionPipeline.Options>
{
    private readonly IMessageService _messages;

    public DialogueExtractionPipeline(IMessageService messages)
    {
        _messages = messages;
    }

    // QnA 쌍을 나타내는 레코드입니다.
    public record Dialogue(string Question, string Answer);

    // 파이프라인 옵션을 나타내는 레코드입니다.
    public record Options(string Provider, string Model);

    // QnA 쌍을 추출하기 위한 정규식입니다.
    [GeneratedRegex(@"<qa>\s*<q>\s*(.*?)\s*</q>\s*<a>\s*(.*?)\s*</a>\s*</qa>", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex DialogueRegex();

    /// <inheritdoc />
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context, 
        Options options, 
        CancellationToken cancellationToken = default)
    {
        if (!context.Payload.TryGetValue<IEnumerable<string>>("chunks", out var chunks))
            throw new InvalidOperationException("chunks is not a IEnumerable<string>");

        var dialogues = new List<Dialogue>();
        foreach (var chunk in chunks)
        {
            var request = new MessageRequest
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
            var result = await _messages.GenerateMessageAsync(request, cancellationToken);
            var text = result.Message?.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value
                ?? throw new InvalidOperationException("No response from the chat completion service.");
            dialogues.AddRange(ParseFrom(text));
        }

        context.Payload.Add("chunks", dialogues);
        return TaskStepResult.Success();
    }

    // 결과 텍스트를 정규식으로 파싱하여 QnA 쌍을 추출합니다.
    private static List<Dialogue> ParseFrom(string text)
    {
        var dialogues = new List<Dialogue>();
        var matches = DialogueRegex().Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 2)
            {
                var question = match.Groups[1].Value.Trim();
                var answer = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(question) && !string.IsNullOrEmpty(answer))
                {
                    dialogues.Add(new Dialogue(question, answer));
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

