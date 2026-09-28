using System.Reflection;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// Every <see cref="MessageGenerationRequest"/> member is a promise to the caller, and each message generator either
/// keeps it (puts it on the wire), refuses it (throws <see cref="NotSupportedException"/>), or leaves it out on
/// purpose for a stated reason. Nothing enforced that. <c>PreviousId</c> was declared on three public types, carried
/// from the agent overlay into the request, promised in the docs as "conversation continuity", and read by no generator
/// at all until 0.44.0 removed it.
/// <para>
/// So this roster counts from the request surface: a new member fails until every generator is classified, and the
/// classification is checked against the generator's source. «Carried» and «Rejected» members must be read there
/// (<c>request.X</c>); «Ignored» members must not be, so a generator that starts reading one also has to update its entry.
/// </para>
/// </summary>
public class RequestFieldRosterTests
{
    private enum Handling { Carried, Rejected, Ignored }

    private sealed record Entry(Handling Handling, string? Reason = null);

    private static Entry Carried => new(Handling.Carried);
    private static Entry Rejected => new(Handling.Rejected);
    private static Entry Ignored(string reason) => new(Handling.Ignored, reason);

    private static readonly Dictionary<string, (string Source, Dictionary<string, Entry> Fields)> Generators = new()
    {
        ["OpenAI (Responses)"] = ("src/IronHive.Providers.OpenAI/OpenAIMessageGenerator.cs", new()
        {
            ["Model"] = Carried, ["System"] = Carried, ["Messages"] = Carried, ["MaxTokens"] = Carried,
            ["Temperature"] = Carried, ["TopP"] = Carried, ["ThinkingEffort"] = Carried, ["ThinkingOutput"] = Carried,
            ["Tools"] = Carried, ["ToolChoice"] = Carried, ["OutputFormat"] = Carried,
            ["ExtraBody"] = Rejected, ["LogProbabilities"] = Rejected,
            ["TopK"] = Ignored("the Responses API has no top_k"),
            ["StopSequences"] = Ignored("the Responses API has no stop parameter"),
        }),
        ["Anthropic"] = ("src/IronHive.Providers.Anthropic/AnthropicMessageGenerator.cs", new()
        {
            ["Model"] = Carried, ["System"] = Carried, ["Messages"] = Carried, ["MaxTokens"] = Carried,
            ["StopSequences"] = Carried, ["ThinkingEffort"] = Carried, ["ThinkingOutput"] = Carried,
            ["Tools"] = Carried, ["ToolChoice"] = Carried, ["OutputFormat"] = Carried,
            ["ExtraBody"] = Rejected, ["LogProbabilities"] = Rejected,
            ["Temperature"] = Ignored("deprecated by Anthropic; newer models answer any value with 400"),
            ["TopP"] = Ignored("deprecated by Anthropic; newer models answer any value with 400"),
            ["TopK"] = Ignored("deprecated by Anthropic; newer models answer any value with 400"),
        }),
        ["Google AI"] = ("src/IronHive.Providers.GoogleAI/GoogleAIMessageGenerator.cs", new()
        {
            ["Model"] = Carried, ["System"] = Carried, ["Messages"] = Carried, ["MaxTokens"] = Carried,
            ["Temperature"] = Carried, ["TopP"] = Carried, ["TopK"] = Carried, ["StopSequences"] = Carried,
            ["ThinkingEffort"] = Carried, ["ThinkingOutput"] = Carried,
            ["Tools"] = Carried, ["ToolChoice"] = Carried, ["OutputFormat"] = Carried,
            ["ExtraBody"] = Rejected, ["LogProbabilities"] = Rejected,
        }),
        ["OpenAI-compatible (Chat Completions)"] = ("src/IronHive.Providers.OpenAI.Compatible/ChatCompletion/ChatCompletionMessageGenerator.cs", new()
        {
            ["Model"] = Carried, ["System"] = Carried, ["Messages"] = Carried, ["MaxTokens"] = Carried,
            ["Temperature"] = Carried, ["TopP"] = Carried, ["TopK"] = Carried, ["StopSequences"] = Carried,
            ["ThinkingEffort"] = Carried, ["Tools"] = Carried, ["ToolChoice"] = Carried, ["OutputFormat"] = Carried,
            ["ExtraBody"] = Carried, ["LogProbabilities"] = Carried,
            ["ThinkingOutput"] = Ignored("Chat Completions has no field for it; reasoning is returned as the server sends it"),
        }),
    };

    public static TheoryData<string> GeneratorNames => [.. Generators.Keys];

    [Theory]
    [MemberData(nameof(GeneratorNames))]
    public void Every_request_member_is_classified_for_the_generator(string generator)
    {
        var members = typeof(MessageGenerationRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet();

        Generators[generator].Fields.Keys.Should().BeEquivalentTo(members,
            "every MessageGenerationRequest member must be classified as carried, rejected or ignored (with a reason)");
    }

    [Theory]
    [MemberData(nameof(GeneratorNames))]
    public void The_classification_matches_what_the_generator_reads(string generator)
    {
        var (source, fields) = Generators[generator];
        var text = File.ReadAllText(Path.Combine(RepositoryRoot().FullName, source));
        var read = Regex.Matches(text, @"\brequest\.([A-Z]\w*)").Select(m => m.Groups[1].Value).ToHashSet();

        foreach (var (field, entry) in fields)
        {
            if (entry.Handling is Handling.Ignored)
            {
                entry.Reason.Should().NotBeNullOrWhiteSpace($"{generator} ignores {field}: say why");
                read.Should().NotContain(field, $"{generator} is listed as ignoring {field} but reads it — update the roster");
            }
            else
            {
                read.Should().Contain(field, $"{generator} is listed as {entry.Handling} for {field} but never reads it");
            }
        }
    }

    private static DirectoryInfo RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            dir = dir.Parent;
        return dir ?? throw new InvalidOperationException("IronHive.slnx not found above the test output directory.");
    }
}
