using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.KernelMemory;
using System.Text;
using Raggle.Server.Web.Options;
using Microsoft.Extensions.Options;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;
using Raggle.Server.Web.Storages;
using Microsoft.SemanticKernel.ChatCompletion;

# pragma warning disable SKEXP0110

namespace Raggle.Server.Web.Services;

public class UserAssistantService
{
    private readonly OpenAIOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppRepository<Assistant> _assistantRepo;
    private VectorStorage? _vector;

    public UserAssistantService(
        IServiceProvider serviceProvider,
        IOptions<OpenAIOptions> options)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _assistantRepo = serviceProvider.GetRequiredService<AppRepository<Assistant>>();
    }

    public async IAsyncEnumerable<string> AskAsync(
        Guid assistantId,
        string query,
        IEnumerable<string>? tags)
    {
        var assistant = await _assistantRepo.GetByIdAsync(assistantId)
            ?? throw new KeyNotFoundException($"Assistant with ID {assistantId} not found.");
        var agent = CreateAgent(assistant);

        //if (sourceIds == null || !sourceIds.Any())
        //{
        //    var sources = await _source.GetAllAsync(userId);
        //    sourceIds = sources.Select(s => s.ID);
        //}

        //var results = new List<SearchReference>();
        //foreach (var sourceId in sourceIds)
        //{
        //    var result = await _vector.SearchAsync(query: query, sourceId.ToString(), null);
        //    results.AddRange(result);
        //}

        //var sortedResults = results
        //   .OrderByDescending(r => r.Relevance)
        //   .Take(10)
        //   .ToList();

        //var groupedResults = sortedResults
        //    .GroupBy(r => r.Name)
        //    .Select(g => new SearchReference
        //    {
        //        Name = g.Key,
        //        Type = g.First().Type,
        //        Relevance = g.Max(r => r.Relevance),
        //        Content = string.Join("\n", g.Select(r => r.Content))
        //    })
        //    .ToList();

        //history.AddSystemMessage($"""
        //    [information]
        //    {JsonSerializer.Serialize(groupedResults)}
        //    """);
        //user.ChatHistory.AddUserMessage(query);
        //history.AddRange(user.ChatHistory);

        var userHistory = assistant.ChatHistory;
        var history = new ChatHistory();
        history.AddRange(userHistory);
        history.AddUserMessage(query);
        var answer = new StringBuilder();
        await foreach (var content in agent.InvokeStreamingAsync(history))
        {
            if (content.Content != null)
            {
                answer.Append(content.Content);
                yield return content.Content;
            }
        }
        history.AddAssistantMessage(answer.ToString());
        assistant.ChatHistory = history;
        await _assistantRepo.UpdateAsync(assistant);
    }

    private ChatCompletionAgent CreateAgent(Assistant assistant)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: _options.ChatModel, apiKey: _options.ApiKey)
            .Build();

        if (assistant?.Connections != null && assistant.Connections.Count > 0)
        {
            // kernel.Plugins.AddFromType<DatabasePlugin>(serviceProvider: _serviceProvider);
        }

        if (assistant?.Knowledges != null && assistant.Knowledges.Count > 0)
        {
            _vector = _serviceProvider.GetRequiredService<VectorStorage>();
        }

        //kernel.Plugins.AddFromType<SearchPlugin>(serviceProvider: service);

        return new ChatCompletionAgent
        {
            Name = "ChatAssistant",
            Description = "Chat Completion Assistant",
            Instructions = assistant?.Instruction,
            Kernel = kernel,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            }
        };
    }

    private string BuildSystemPrompt(IEnumerable<SearchResult> searchResults)
    {
        if (searchResults.Count() == 0)
        {
            return "No search results found.";
        }

        var prompt = new StringBuilder();
        prompt.AppendLine("I found the following search results:");
        foreach (var result in searchResults)
        {
        }
        return prompt.ToString();
    }
}
