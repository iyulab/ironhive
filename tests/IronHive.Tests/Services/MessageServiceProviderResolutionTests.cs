using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Registries;
using IronHive.Core.Services;
using NSubstitute;

namespace IronHive.Tests.Services;

public class MessageServiceProviderResolutionTests
{
    private static MessageRequest MakeRequest(string provider = "", string model = "gpt-4o") =>
        new()
        {
            Provider = provider,
            Model = model,
            Messages = [new UserMessage { Content = [new TextMessageContent { Value = "hi" }] }],
            Tools = []
        };

    private static MessageResponse MakeResponse() => new()
    {
        Id = "r1",
        DoneReason = MessageDoneReason.EndTurn,
        Message = new AssistantMessage { Content = [new TextMessageContent { Value = "ok" }] }
    };

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_SingleGenerator_AutoSelects()
    {
        var generator = Substitute.For<IMessageGenerator>();
        generator.GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(MakeResponse());

        var providerRegistry = new ProviderRegistry();
        providerRegistry.TryAdd<IMessageGenerator>("openai", generator);

        var tools = Substitute.For<IToolCollection>();
        tools.FilterBy(Arg.Any<IEnumerable<string>>()).Returns(tools);

        var services = Substitute.For<IServiceProvider>();
        var svc = new MessageService(services, providerRegistry, tools);

        var result = await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        result.Should().NotBeNull();
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
    }

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_NoGenerator_Throws()
    {
        var providerRegistry = new ProviderRegistry();
        var tools = Substitute.For<IToolCollection>();
        tools.FilterBy(Arg.Any<IEnumerable<string>>()).Returns(tools);

        var services = Substitute.For<IServiceProvider>();
        var svc = new MessageService(services, providerRegistry, tools);

        var act = async () => await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No message generators*");
    }

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_MultipleGenerators_Throws()
    {
        var providerRegistry = new ProviderRegistry();
        providerRegistry.TryAdd<IMessageGenerator>("openai", Substitute.For<IMessageGenerator>());
        providerRegistry.TryAdd<IMessageGenerator>("anthropic", Substitute.For<IMessageGenerator>());

        var tools = Substitute.For<IToolCollection>();
        tools.FilterBy(Arg.Any<IEnumerable<string>>()).Returns(tools);

        var services = Substitute.For<IServiceProvider>();
        var svc = new MessageService(services, providerRegistry, tools);

        var act = async () => await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Specify a provider*");
    }
}
