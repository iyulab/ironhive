using Raggle.Abstractions.Assistant;

namespace Raggle.Core.Assistant;

internal class RaggleAssistant : IRaggleAssistant
{
    public IServiceProvider Services { get; }

    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }

    public RaggleAssistant(IServiceProvider services)
    {
        Services = services;
    }

    public Task<string> ChatCompletionAsync(string prompt)
    {
        throw new NotImplementedException();
    }
}
