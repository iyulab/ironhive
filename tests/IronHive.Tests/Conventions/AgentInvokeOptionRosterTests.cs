using System.Reflection;
using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent;
using IronHive.Core.Tools;
using NSubstitute;

namespace IronHive.Tests.Conventions;

/// <summary>
/// <see cref="AgentInvokeOptions"/> is a per-invoke overlay: every knob it declares is a promise that
/// the value reaches the request the agent actually sends. Nothing enforced that promise — the overlay
/// is a hand-written list of assignments, and a knob added to the options type without a matching line
/// is dropped in silence, which the caller sees as the agent ignoring what they asked for.
/// <para>
/// The same gap on the <c>IChatClient</c> bridge held two live defects (a JSON response format and
/// options-level instructions, both with a sink waiting under another name), and it was invisible there
/// for the same reason it is invisible here: the scan ran from the sink side, where a renamed pair looks
/// like nothing at all. So this counts from the option surface — every knob is mapped or recorded as
/// deliberately not carried, and a new one fails until someone decides which it is.
/// </para>
/// </summary>
public class AgentInvokeOptionRosterTests
{
    /// <summary>AgentInvokeOptions knob → the MessageRequest field the overlay writes it to.</summary>
    private static readonly Dictionary<string, string> OptionSinks = new()
    {
        ["PreviousId"] = "PreviousId",
        ["ThinkingEffort"] = "ThinkingEffort",
        ["ThinkingOutput"] = "ThinkingOutput",
        ["MaxTokens"] = "MaxTokens",
        ["Temperature"] = "Temperature",
        ["TopP"] = "TopP",
        ["TopK"] = "TopK",
        ["StopSequences"] = "StopSequences",
        ["Tools"] = "Tools",
        ["ToolOptions"] = "ToolOptions",
        ["OutputFormat"] = "OutputFormat",
        ["Suggestions"] = "Suggestions",
        ["MaxTurns"] = "MaxTurns",
        ["Items"] = "Items",
        ["ExtraBody"] = "ExtraBody",
        ["LogProbabilities"] = "LogProbabilities"
    };

    /// <summary>
    /// Knobs the overlay deliberately does not carry, each with the reason. Empty today — every knob on
    /// the options type is carried — and the table exists so that staying empty is a decision rather
    /// than an accident.
    /// </summary>
    private static readonly Dictionary<string, string> DeliberatelyUnmapped = new();

    [Fact]
    public void EveryInvokeOption_IsEitherMappedOrDeliberatelyNot()
    {
        var unaccounted = typeof(AgentInvokeOptions).GetProperties()
            .Select(p => p.Name)
            .Where(knob => !OptionSinks.ContainsKey(knob) && !DeliberatelyUnmapped.ContainsKey(knob))
            .ToList();

        unaccounted.Should().BeEmpty(
            "every AgentInvokeOptions knob must be applied in BasicAgent's overlay (and listed in " +
            "OptionSinks) or recorded in DeliberatelyUnmapped with the reason it is not carried — a knob " +
            "in neither is dropped silently, and the caller only sees the agent ignore the request");

        OptionSinks.Keys.Should().NotIntersectWith(DeliberatelyUnmapped.Keys,
            "a knob cannot be both carried and deliberately not carried");
    }

    [Fact]
    public void EveryMappedSink_ExistsOnTheRequest()
    {
        var missing = OptionSinks.Values
            .Where(sink => typeof(MessageRequest).GetProperty(sink) is null)
            .ToList();

        missing.Should().BeEmpty("a sink named here has to exist, or the roster is describing a mapping " +
            "that cannot be true");
    }

    [Fact]
    public async Task EveryMappedOption_ReachesItsRequestSink()
    {
        // Each knob is set on its own so a sink filled from the agent's own defaults, or from the wrong
        // source, cannot make another knob look carried.
        foreach (var (optionName, sinkName) in OptionSinks)
        {
            var messageService = Substitute.For<IMessageService>();
            MessageRequest? captured = null;
            messageService
                .GenerateMessageAsync(Arg.Do<MessageRequest>(r => captured = r), Arg.Any<CancellationToken>())
                .Returns(new MessageResponse
                {
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = new Message { Role = MessageRole.Assistant, Content = [] }
                });

            var agent = new BasicAgent(messageService) { Provider = "openai", Model = "gpt-4o" };
            var options = new AgentInvokeOptions();
            var optionProperty = typeof(AgentInvokeOptions).GetProperty(optionName)!;
            optionProperty.SetValue(options, SampleValueFor(optionProperty.PropertyType));

            await agent.InvokeAsync([Message.User("Hi")], options, TestContext.Current.CancellationToken);

            captured.Should().NotBeNull();
            var sink = typeof(MessageRequest).GetProperty(sinkName)!;
            sink.GetValue(captured).Should().NotBeNull(
                $"AgentInvokeOptions.{optionName} must reach MessageRequest.{sinkName} — a dropped overlay " +
                "is silent: the caller sees no error, only the agent's own default");
        }
    }

    private static object SampleValueFor(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (type == typeof(string)) return "sample";
        if (type == typeof(int)) return 7;
        if (type == typeof(float)) return 0.5f;
        if (type == typeof(MessageThinkingEffort)) return MessageThinkingEffort.Low;
        if (type == typeof(MessageThinkingOutput)) return MessageThinkingOutput.Summary;
        if (type == typeof(OutputFormat)) return OutputFormat.For("""{"type":"object"}""");
        if (type == typeof(ToolOptions)) return new ToolOptions();
        if (type == typeof(SuggestionOptions)) return new SuggestionOptions();
        if (type == typeof(MessageContextItems)) return new MessageContextItems();
        if (type == typeof(LogProbabilityOptions)) return new LogProbabilityOptions { TopAlternatives = 3 };
        if (type == typeof(System.Text.Json.Nodes.JsonObject)) return new System.Text.Json.Nodes.JsonObject { ["sample"] = 1 };
        if (typeof(IToolCollection).IsAssignableFrom(type)) return new ToolCollection();
        if (typeof(ICollection<string>).IsAssignableFrom(type)) return new List<string> { "STOP" };

        throw new NotSupportedException(
            $"No sample value for {propertyType} — extend SampleValueFor when adding a knob of a new type");
    }
}
