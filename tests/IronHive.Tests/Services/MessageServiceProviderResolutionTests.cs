using FluentAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
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
            Messages = [Message.User("hi")],
        };

    private static MessageResponse MakeResponse() => new()
    {
        ResponseId = "r1",
        DoneReason = MessageDoneReason.EndTurn,
        Message = Message.Assistant("ok")
    };

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_SingleGenerator_AutoSelects()
    {
        var generator = Substitute.For<IMessageGenerator>();
        generator.GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(MakeResponse());

        var generators = new Dictionary<string, IMessageGenerator> { ["openai"] = generator };
        var svc = new MessageService(generators);

        var result = await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        result.Should().NotBeNull();
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
    }

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_NoGenerator_Throws()
    {
        var svc = new MessageService(new Dictionary<string, IMessageGenerator>());

        var act = async () => await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No message generators*");
    }

    [Fact]
    public async Task GenerateMessageAsync_EmptyProvider_MultipleGenerators_Throws()
    {
        var generators = new Dictionary<string, IMessageGenerator>
        {
            ["openai"] = Substitute.For<IMessageGenerator>(),
            ["anthropic"] = Substitute.For<IMessageGenerator>()
        };
        var svc = new MessageService(generators);

        var act = async () => await svc.GenerateMessageAsync(MakeRequest(provider: ""));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Specify a provider*");
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_EmptyProvider_SingleGenerator_AutoSelects()
    {
        var generator = Substitute.For<IMessageGenerator>();
        generator.GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StreamingMessageResponse>());

        var generators = new Dictionary<string, IMessageGenerator> { ["openai"] = generator };
        var svc = new MessageService(generators);

        var act = async () =>
        {
            await foreach (var _ in svc.GenerateStreamingMessageAsync(MakeRequest(provider: ""))) { }
        };
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_EmptyProvider_NoGenerator_Throws()
    {
        var svc = new MessageService(new Dictionary<string, IMessageGenerator>());

        var act = async () =>
        {
            await foreach (var _ in svc.GenerateStreamingMessageAsync(MakeRequest(provider: ""))) { }
        };

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No message generators*");
    }

    [Fact]
    public async Task GenerateStreamingMessageAsync_EmptyProvider_MultipleGenerators_Throws()
    {
        var generators = new Dictionary<string, IMessageGenerator>
        {
            ["openai"] = Substitute.For<IMessageGenerator>(),
            ["anthropic"] = Substitute.For<IMessageGenerator>()
        };
        var svc = new MessageService(generators);

        var act = async () =>
        {
            await foreach (var _ in svc.GenerateStreamingMessageAsync(MakeRequest(provider: ""))) { }
        };

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Specify a provider*");
    }
}
