using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Raggle.Server.API.Models;

# pragma warning disable SKEXP0110

namespace Raggle.Server.API.Assistant;

public class SearchAssistant
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger<SearchAssistant> _logger;

    public SearchAssistant(IConfiguration config, ILogger<SearchAssistant> logger)
    {
        _logger = logger;
        var option = config.GetSection("OpenAI").Get<OpenAIOptions>();
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: option.ChatModel, apiKey: option.ApiKey)
            .Build();

        _agent = new ChatCompletionAgent
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

    public async IAsyncEnumerable<string> Search(ChatHistory history)
    {
        await foreach (var item in _agent.InvokeStreamingAsync(history))
        {
            yield return item.Content;
        }
    }
}
