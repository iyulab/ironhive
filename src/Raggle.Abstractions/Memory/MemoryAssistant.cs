using Raggle.Abstractions.AI;

namespace Raggle.Abstractions.Memory;

public class MemoryAssistant
{

    public string Model { get; set; }

    public IChatCompletionService Service { get; set; }

    public MemoryAssistant(string model, IChatCompletionService service)
    {
        Model = model;
        Service = service;
    }

    public async Task<IEnumerable<ChatCompletionResponse>> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
