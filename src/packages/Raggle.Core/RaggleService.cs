using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.ChatCompletion;
using Raggle.Abstractions;
using Raggle.Abstractions.Prompts;
using System.Text;

namespace Raggle.Core;

public class RaggleService : IRaggleService
{
    private const string DEFAULT_INDEX = "index_test_tt";
    private readonly IChatCompletionService _chat;
    private readonly IKernelMemory _memory;
    private readonly IPromptProvider? _prompt;
    private readonly ChatHistory _history;

    public RaggleService(
        IChatCompletionService chatService, 
        IKernelMemory kernelMemory,
        IPromptProvider? promptProvider = null)
    {
        _memory = kernelMemory;
        _chat = chatService;
        _prompt = promptProvider;
        _history = new(promptProvider?.GetPrompt() ?? string.Empty);
    }

    public async Task<string> MemorizeTextAsync(string documentId, string text, string? index = null)
    {
        index ??= DEFAULT_INDEX;
        return await _memory.ImportTextAsync(text: text, documentId: documentId, index: index);
    }

    public async Task<string> MemorizeDocumentAsync(string documentId, string filePath, string? index = null)
    {
        index ??= DEFAULT_INDEX;
        var isExist = await _memory.IsDocumentReadyAsync(documentId: documentId, index: index);
        return isExist
            ? documentId
            : await _memory.ImportDocumentAsync(filePath: filePath, documentId: documentId, index: index);
    }

    public async Task<string> MemorizeWebPageAsync(string documentId, string url, string? index = null)
    {
        index ??= DEFAULT_INDEX;
        var isExist = await _memory.IsDocumentReadyAsync(documentId: documentId, index: index);
        return isExist
            ? documentId
            : await _memory.ImportWebPageAsync(url: url, documentId: documentId, index: index);
    }

    public async Task UnMemorizeAsync(string documentId, string? index = null)
    {
        index ??= DEFAULT_INDEX;
        await _memory.DeleteDocumentAsync(documentId: documentId, index: index);
    }

    public async Task<DataPipelineStatus?> GetEmbeddingStatusAsync(string documentId, string? index = null)
    {
        index ??= DEFAULT_INDEX;
        return await _memory.GetDocumentStatusAsync(documentId: documentId, index: index);
    }

    public async Task<string> GetInformationAsync(
        string query, 
        int? limit = null, 
        double? minRelevance = null, 
        string? index = null,
        ICollection<MemoryFilter>? filters = null)
    {
        index ??= DEFAULT_INDEX;
        var memories = await _memory.SearchAsync(
                query: query,
                index: index,
                limit: limit ?? 10,
                minRelevance: minRelevance ?? 0,
                filters: filters
            );
        return memories.Results.SelectMany(m => m.Partitions)
            .Aggregate("", (sum, chunk) => sum + chunk.Text + "\n")
            .Trim();
    }

    public async IAsyncEnumerable<string> AskStreamingAsync(
        string query, 
        ICollection<MemoryFilter>? filters = null)
    {
        var information = await GetInformationAsync(query: query, filters: filters);
        _history[0].Content = _prompt?.GetPromptWithInfo(information) ?? string.Empty;
        _history.AddUserMessage(query);
        var reply = new StringBuilder();
        await foreach (var stream in _chat.GetStreamingChatMessageContentsAsync(_history))
        {
            var content = stream.Content;
            if (content is not null)
            {
                reply.Append(content);
                yield return content;
            }
        }
        _history.AddAssistantMessage(reply.ToString());
    }

    public void ClearHistory()
    {
        _history.Clear();
        _history.AddSystemMessage(_prompt?.GetPrompt() ?? string.Empty);
    }
}
