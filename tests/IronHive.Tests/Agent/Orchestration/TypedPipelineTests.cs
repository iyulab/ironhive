using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using System.Globalization;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class TypedPipelineTests
{
    #region AgentExecutor

    [Fact]
    public void AgentExecutor_NullAgent_ThrowsArgumentNullException()
    {
        var act = () => new AgentExecutor<string, string>(
            null!,
            _ => [new UserMessage { Content = [new TextMessageContent { Value = _ }] }],
            r => "ok");

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("agent");
    }

    [Fact]
    public void AgentExecutor_NullInputConverter_ThrowsArgumentNullException()
    {
        var agent = new MockAgent("test");

        var act = () => new AgentExecutor<string, string>(
            agent,
            null!,
            r => "ok");

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("inputConverter");
    }

    [Fact]
    public void AgentExecutor_NullOutputConverter_ThrowsArgumentNullException()
    {
        var agent = new MockAgent("test");

        var act = () => new AgentExecutor<string, string>(
            agent,
            _ => [new UserMessage { Content = [new TextMessageContent { Value = _ }] }],
            null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("outputConverter");
    }

    [Fact]
    public void AgentExecutor_Name_ReturnsAgentName()
    {
        var agent = new MockAgent("my-agent");
        var executor = CreateStringExecutor(agent);

        executor.Name.Should().Be("my-agent");
    }

    [Fact]
    public async Task AgentExecutor_ExecuteAsync_CallsAgentAndConvertsOutput()
    {
        var agent = new MockAgent("echo") { ResponseFunc = _ => "response-text" };
        var executor = CreateStringExecutor(agent);

        var result = await executor.ExecuteAsync("hello");

        result.Should().Be("response-text");
    }

    [Fact]
    public async Task AgentExecutor_ExecuteAsync_PassesInputToAgent()
    {
        string? receivedInput = null;
        var agent = new MockAgent("capture")
        {
            ResponseFunc = msgs =>
            {
                var msg = msgs.OfType<UserMessage>().First();
                var text = msg.Content?.OfType<TextMessageContent>().First();
                receivedInput = text?.Value;
                return "ok";
            }
        };
        var executor = CreateStringExecutor(agent);

        await executor.ExecuteAsync("test-input");

        receivedInput.Should().Be("test-input");
    }

    #endregion

    #region TypedPipeline.Start + Build

    [Fact]
    public async Task Start_SingleExecutor_BuildReturnsExecutor()
    {
        var executor = new SimpleExecutor<int, string>("first", i => $"value:{i}");

        var pipeline = TypedPipeline.Start(executor).Build();

        pipeline.Name.Should().Be("first");
        var result = await pipeline.ExecuteAsync(42);
        result.Should().Be("value:42");
    }

    #endregion

    #region TypedPipelineBuilder.Then + Chaining

    [Fact]
    public async Task Then_TwoExecutors_ChainsOutputToInput()
    {
        var first = new SimpleExecutor<string, int>("parse", s => int.Parse(s, CultureInfo.InvariantCulture));
        var second = new SimpleExecutor<int, string>("format", i => $"result:{i * 2}");

        var pipeline = TypedPipeline.Start(first).Then(second).Build();

        var result = await pipeline.ExecuteAsync("5");
        result.Should().Be("result:10");
    }

    [Fact]
    public async Task Then_ThreeExecutors_ChainsAll()
    {
        var first = new SimpleExecutor<string, int>("step1", s => s.Length);
        var second = new SimpleExecutor<int, double>("step2", i => i * 1.5);
        var third = new SimpleExecutor<double, string>("step3", d => d.ToString("F1", CultureInfo.InvariantCulture));

        var pipeline = TypedPipeline
            .Start(first)
            .Then(second)
            .Then(third)
            .Build();

        var result = await pipeline.ExecuteAsync("hello");
        result.Should().Be("7.5"); // "hello".Length=5, 5*1.5=7.5
    }

    [Fact]
    public void ChainedExecutor_Name_CombinesNames()
    {
        var first = new SimpleExecutor<string, int>("A", _ => 0);
        var second = new SimpleExecutor<int, string>("B", _ => "");

        var pipeline = TypedPipeline.Start(first).Then(second).Build();

        pipeline.Name.Should().Be("A -> B");
    }

    [Fact]
    public void ChainedExecutor_ThreeSteps_Name_ChainsAll()
    {
        var a = new SimpleExecutor<int, int>("A", x => x);
        var b = new SimpleExecutor<int, int>("B", x => x);
        var c = new SimpleExecutor<int, int>("C", x => x);

        var pipeline = TypedPipeline.Start(a).Then(b).Then(c).Build();

        pipeline.Name.Should().Be("A -> B -> C");
    }

    [Fact]
    public async Task Pipeline_WithAgentExecutor_IntegrationTest()
    {
        var agent = new MockAgent("summarizer") { ResponseFunc = _ => "summary" };
        var agentExecutor = CreateStringExecutor(agent);
        var formatter = new SimpleExecutor<string, string>("fmt", s => $"[{s}]");

        var pipeline = TypedPipeline
            .Start(agentExecutor)
            .Then(formatter)
            .Build();

        var result = await pipeline.ExecuteAsync("raw document text");
        result.Should().Be("[summary]");
    }

    [Fact]
    public async Task Pipeline_CancellationToken_PropagatedToExecutors()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var executor = new SimpleExecutor<string, string>("check", _ =>
            throw new InvalidOperationException("should not reach here"));

        var cancellingFirst = new CancellationAwareExecutor<string, string>("cancel-check");

        var pipeline = TypedPipeline.Start(cancellingFirst).Then(executor).Build();

        var act = async () => await pipeline.ExecuteAsync("input", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Test Helpers

    private static AgentExecutor<string, string> CreateStringExecutor(IAgent agent)
    {
        return new AgentExecutor<string, string>(
            agent,
            input => [new UserMessage { Content = [new TextMessageContent { Value = input }] }],
            response =>
            {
                var msg = response.Message as AssistantMessage;
                var text = msg?.Content?.OfType<TextMessageContent>().FirstOrDefault();
                return text?.Value ?? "";
            });
    }

    private sealed class SimpleExecutor<TIn, TOut> : ITypedExecutor<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _func;

        public string Name { get; }

        public SimpleExecutor(string name, Func<TIn, TOut> func)
        {
            Name = name;
            _func = func;
        }

        public Task<TOut> ExecuteAsync(TIn input, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_func(input));
        }
    }

    private sealed class CancellationAwareExecutor<TIn, TOut> : ITypedExecutor<TIn, TOut>
    {
        public string Name { get; }

        public CancellationAwareExecutor(string name) { Name = name; }

        public Task<TOut> ExecuteAsync(TIn input, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(default(TOut)!);
        }
    }

    private sealed class MockAgent : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; }
        public string Description { get; set; } = "Mock";
        public string? Instructions { get; set; }
        public IEnumerable<ToolItem>? Tools { get; set; }
        public MessageGenerationParameters? Parameters { get; set; }
        public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

        public MockAgent(string name) { Name = name; }

        public Task<MessageResponse> InvokeAsync(IEnumerable<Message> messages, CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";
            return Task.FromResult(new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = Name,
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Yield();
            yield return new StreamingMessageBeginResponse { Id = Guid.NewGuid().ToString("N") };
        }
    }

    #endregion
}
