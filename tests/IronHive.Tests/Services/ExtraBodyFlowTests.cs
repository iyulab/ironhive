using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Microsoft;
using IronHive.Core.Services;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// <see cref="MessageRequest.ExtraBody"/> reaches the generator, and a generator's response <c>ExtraBody</c> reaches the
/// caller — through <see cref="MessageService"/> on both paths, and through the <see cref="IChatClient"/> bridge as
/// <c>AdditionalProperties</c>.
/// </summary>
public class ExtraBodyFlowTests
{
    private static JsonObject Timings() => new() { ["timings"] = new JsonObject { ["predicted_ms"] = 7.25 } };

    [Fact]
    public async Task MessageService_Buffered_CarriesBothDirections()
    {
        var generator = Substitute.For<IMessageGenerator>();
        MessageGenerationRequest? seen = null;
        generator.GenerateMessageAsync(Arg.Do<MessageGenerationRequest>(r => seen = r), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { DoneReason = MessageDoneReason.EndTurn, Message = Message.Assistant("yes"), ExtraBody = Timings() });
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { ["p"] = generator });

        var response = await service.GenerateMessageAsync(new MessageRequest
        {
            Provider = "p",
            Model = "m",
            Messages = [Message.User("Hi")],
            ExtraBody = new JsonObject { ["n_probs"] = 20 },
        }, TestContext.Current.CancellationToken);

        seen!.ExtraBody!["n_probs"]!.GetValue<int>().Should().Be(20);
        response.ExtraBody!["timings"]!["predicted_ms"]!.GetValue<double>().Should().Be(7.25);
    }

    [Fact]
    public async Task MessageService_Streaming_CarriesTheDoneFramesExtraBody()
    {
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { ["p"] = new DoneOnlyGenerator(Timings()) });

        StreamingMessageDoneResponse? done = null;
        await foreach (var frame in service.GenerateStreamingMessageAsync(
            new MessageRequest { Provider = "p", Model = "m", Messages = [Message.User("Hi")] }, TestContext.Current.CancellationToken))
        {
            if (frame is StreamingMessageDoneResponse d)
                done = d;
        }

        done!.ExtraBody!["timings"]!["predicted_ms"]!.GetValue<double>().Should().Be(7.25);
    }

    [Fact]
    public async Task ChatClientBridge_MapsExtraBodyToAdditionalProperties()
    {
        var generator = Substitute.For<IMessageGenerator>();
        generator.GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse { DoneReason = MessageDoneReason.EndTurn, Message = Message.Assistant("yes"), ExtraBody = Timings() });
        using var client = new ChatClientAdapter(generator, "m");

        var response = await client.GetResponseAsync("Hi", cancellationToken: TestContext.Current.CancellationToken);

        var timings = (JsonElement)response.AdditionalProperties!["timings"]!;
        timings.GetProperty("predicted_ms").GetDouble().Should().Be(7.25);
    }

    private sealed class DoneOnlyGenerator(JsonObject extraBody) : IMessageGenerator
    {
        public Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
            MessageGenerationRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            yield return new StreamingMessageDoneResponse { DoneReason = MessageDoneReason.EndTurn, ExtraBody = extraBody };
        }

        public Task<int> CountTokensAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose() { }
    }
}
