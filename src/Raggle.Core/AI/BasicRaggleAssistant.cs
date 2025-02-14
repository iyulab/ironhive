using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;

namespace Raggle.Core.AI;

internal class BasicRaggleAssistant : IAssistant
{
    private readonly IServiceProvider _provider;

    public required string Service { get; set; }

    public required string Model { get; set; }

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Instruction { get; set; }

    public ChatCompletionParameters? Options { get; set; }

    public ToolCollection? Tools { get; set; }

    public BasicRaggleAssistant(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<MessageResponse> InvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(Service);
        return service.ChatCompletionAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<StreamingMessageResponse> StreamingInvokeAsync(
        MessageCollection messages,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages);
        var service = _provider.GetRequiredKeyedService<IChatCompletionService>(Service);
        return service.StreamingChatCompletionAsync(request, cancellationToken);
    }

    private MessageContext BuildRequest(
        MessageCollection messages)
    {
        return new MessageContext
        {
            Model = Model,
            Messages = messages,
            Parameters = Options,
            Tools = Tools,
            MessagesOptions = new MessageOptions
            {
                System = Instruction,
                Strategy = MessageStrategy.Summarize
            },
            ToolOptions = new ToolExecutionOptions
            {
                SelectionMode = ToolSelectionMode.Auto
            }
        };
    }
}
