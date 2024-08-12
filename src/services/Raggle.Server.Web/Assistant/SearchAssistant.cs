using Azure.Search.Documents.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.KernelMemory;
using Raggle.Server.API.Models;
using Raggle.Server.API.Storages;
using Raggle.Server.Web.Repositories;
using System.Text;
using Raggle.Server.API.Repositories;
using Raggle.Server.Web.Assistant;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.Json;

# pragma warning disable SKEXP0110

namespace Raggle.Server.API.Assistant;

public class SearchAssistant
{
    private readonly ChatCompletionAgent _agent;
    private readonly VectorStorage _vector;
    private readonly UserRepository _user;
    private readonly SourceRepository _source;

    public SearchAssistant(
        IConfiguration config,
        IServiceProvider serviceProvider,
        VectorStorage vectorStorage, 
        UserRepository userRepository,
        SourceRepository source)
    {
        var option = config.GetSection("OpenAI").Get<OpenAIOptions>();
        _agent = CreateNewAgent(option, serviceProvider);
        _vector = vectorStorage;
        _user = userRepository;
        _source = source;
    }

    private ChatCompletionAgent CreateNewAgent(OpenAIOptions options, IServiceProvider service)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: options.ChatModel, apiKey: options.ApiKey)
            .Build();
        //kernel.Plugins.AddFromType<SearchPlugin>(serviceProvider: service);

        return new ChatCompletionAgent
        {
            Name = "OpenAI",
            Description = "OpenAI Chat Completion",
            Instructions = "Ask me anything!",
            Kernel = kernel,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            }
        };
    }

    public async IAsyncEnumerable<string> AskAsync(
        Guid userId,
        string query,
        IEnumerable<Guid>? sourceIds)
    {
        var user = await _user.GetAsync(userId);
        
        if (sourceIds == null || !sourceIds.Any())
        {
            var sources = await _source.GetAllAsync(userId);
            sourceIds = sources.Select(s => s.ID);
        }
        
        var results = new List<SearchReference>();
        foreach (var sourceId in sourceIds)
        {
            var result = await _vector.SearchAsync(query: query, sourceId.ToString(), null);
            results.AddRange(result);
        }

        var sortedResults = results
           .OrderByDescending(r => r.Relevance)
           .Take(10)
           .ToList();

        var groupedResults = sortedResults
            .GroupBy(r => r.Name)
            .Select(g => new SearchReference
            {
                Name = g.Key,
                Type = g.First().Type,
                Relevance = g.Max(r => r.Relevance),
                Content = string.Join("\n", g.Select(r => r.Content))
            })
            .ToList();

        var history = new ChatHistory();
        history.AddSystemMessage($"""
            [information]
            {JsonSerializer.Serialize(groupedResults)}
            """);
        user.ChatHistory.AddUserMessage(query);
        history.AddRange(user.ChatHistory);
        
        var answer = new StringBuilder();
        await foreach (var content in _agent.InvokeStreamingAsync(history))
        {
            if (content.Content != null)
            {
                answer.Append(content.Content);
                yield return content.Content;
            }
        }
        user.ChatHistory.AddAssistantMessage(answer.ToString());

        await _user.UpdateAsync(user.ID, JsonSerializer.SerializeToElement(new
        {
            chatHistory = user.ChatHistory
        }));
    }

    private string BuildSystemPrompt(IEnumerable<SearchResult> searchResults)
    {
        if(searchResults.Count() == 0)
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

public class StreamMessage
{
    public IEnumerable<SearchReference> References { get; set; }
    public string Content { get; set; }
}

public class SearchReference
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double Relevance { get; set; }
    public string Content { get; set; }
}