using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Messages;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace IronHive.Core;

public class MessageContext
{
    public string? Title { get; set; }

    public string? Summary { get; set; }

    public MessageCollection Messages { get; set; } = [];

    public string? ChoiceAgent { get; set; }

    public int? LastSummarizedIndex { get; set; }

    public int? TotalTokens { get; set; }
}

public class SessionResult
{

}

public class HiveSession : IHiveSession
{
    private readonly IChatCompletionService _serivce;    

    public required IHiveAgent Master { get; init; }

    public required IDictionary<string, IHiveAgent> Agents { get; init; }

    public HiveSession(IServiceProvider services)
    {
        _serivce = services.GetRequiredService<IChatCompletionService>();
    }

    public async Task<SessionResult> InvokeAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        if (context.Title == null)
        {
            context.Title = await GenerateTitleAsync(context.Messages, cancellationToken);
        }

        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<SessionResult> InvokeStreamingAsync(MessageContext context, CancellationToken cancellationToken = default)
    {
        if (context.Title == null)
        {
            context.Title = await GenerateTitleAsync(context.Messages, cancellationToken);
        }

        throw new NotImplementedException();
    }

    private async Task<string> GenerateTitleAsync(MessageCollection messages, CancellationToken cancellationToken = default)
    {
        var options = new ChatCompletionOptions
        {
            Provider = Master.Provider,
            Model = Master.Model,
            Instructions = GetTitleInstructions(),
            MaxTokens = 100,
            Temperature = 0.5f,
            TopK = 40,
            TopP = 0.9f,
        };

        var message = new UserMessage();
        message.Content.Add(new UserTextContent
        {
            Value = $"Generate title this following messages:\n {JsonSerializer.Serialize(messages)}",
        });
        var result = await _serivce.GenerateMessageAsync(message, options, cancellationToken);
        var content = result.Data?.Content.OfType<AssistantTextContent>().FirstOrDefault();
        var title = content?.Value;
        if (string.IsNullOrEmpty(title))
        {
            throw new InvalidOperationException("Failed to generate title.");
        }
        return title;
    }

    private string GetTitleInstructions()
    {
        return """
        You are a title generator.
        - Provide a brief and catchy title for the conversation.
        - Focus on the main theme or topic of the conversation.
        - Use simple and clear language.
        - Avoid jargon or complex terms.
        - Keep the title concise and to the point.
        """;
    }

    private async Task<string> GenerateSummaryAsync(MessageCollection messages, CancellationToken cancellationToken = default)
    {
        var options = new ChatCompletionOptions
        {
            Provider = Master.Provider,
            Model = Master.Model,
            Instructions = GetSummaryInstructions(),
            MaxTokens = 2000,
            Temperature = 0.5f,
            TopK = 40,
            TopP = 0.9f,
        };

        var message = new UserMessage();
        message.Content.Add(new UserTextContent
        {
            Value = $"Summarize this following messages:\n {JsonSerializer.Serialize(messages)}",
        });
        var result = await _serivce.GenerateMessageAsync(message, options, cancellationToken);
        var content = result.Data?.Content.OfType<AssistantTextContent>().FirstOrDefault();
        var summary = content?.Value;
        if (string.IsNullOrEmpty(summary))
        {
            throw new InvalidOperationException("Failed to generate summary.");
        }
        return summary;
    }

    private string GetSummaryInstructions()
    {
        return """
        You are a summarizer.
        - Provide a brief summary of the conversation.
        - Focus on the main points and any key details.
        - Do not include personal opinions or irrelevant information.
        - Use simple and clear language.
        - Avoid jargon or complex terms.
        - Present the information in bullet points or numbered lists.
        - Keep the summary concise and to the point.
        - Use active voice and present tense.
        """;
    }

    private ChatCompletionOptions BuildOptions(IHiveAgent agent)
    {
        return new ChatCompletionOptions
        {
            Provider = agent.Provider,
            Model = agent.Model,
            Instructions = agent.Instructions,
            Tools = agent.Tools,
            MaxTokens = agent.Parameters?.MaxTokens,
            StopSequences = agent.Parameters?.StopSequences,
            Temperature = agent.Parameters?.Temperature,
            TopP = agent.Parameters?.TopP,
            TopK = agent.Parameters?.TopK,
        };
    }
}
