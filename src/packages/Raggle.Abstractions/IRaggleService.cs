

using Microsoft.KernelMemory;

namespace Raggle.Abstractions;

public interface IRaggleService
{
    IAsyncEnumerable<string> AskStreamingAsync(string query, ICollection<MemoryFilter>? filters = null);
    void ClearHistory();
    Task<DataPipelineStatus?> GetMemorizeStatusAsync(string documentId, string? index = null);
    Task<string> GetInformationAsync(string query, int? limit = null, double? minRelevance = null, string? index = null, ICollection<MemoryFilter>? filters = null);
    Task<string> MemorizeDocumentAsync(string documentId, string filePath, string? index = null);
    Task<string> MemorizeTextAsync(string documentId, string text, string? index = null);
    Task<string> MemorizeWebPageAsync(string documentId, string url, string? index = null);
    Task UnMemorizeAsync(string documentId, string? index = null);
}
