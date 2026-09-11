using System.Reflection;
using AwesomeAssertions;

namespace IronHive.Tests.Conventions;

// Teeth for the "two entry points that must agree share the reconstruction" convention
// (see ironhive-umbrella docs/CONVENTIONS.md §5).
//
// The dominant shape of that defect in this ecosystem is a streaming/non-streaming pair:
// one operation exposed twice, where the streaming half quietly reconstructs the result
// on its own and drifts from the buffered half. Three separate instances were found this
// way -- tool results never collected, usage never aggregated, tool results dropped from
// history -- and all three were passing CI at the time, because no test asked the two
// halves the same question.
//
// A pair is statically findable: the same declaring type exposing `XAsync` and
// `XStreamingAsync`. This test pins the set of pairs so that a NEW pair cannot appear
// without someone deciding, in that moment, what "the two agree" means for it and writing
// a test that asserts it. It deliberately does not try to verify equivalence itself --
// equivalence is per-operation and belongs in that operation's own tests.
public class StreamingPairRosterTests
{
    private const string StreamingToken = "Streaming";
    private const string AsyncSuffix = "Async";

    // Every pair below has, or must gain, a test asserting that the two halves produce the
    // same logical result. Adding a row here without that test defeats the point.
    private static readonly string[] KnownPairs =
    [
        "IAgent.Invoke",
        "IAgentMiddleware+IStreamingAgentMiddleware.Invoke",
        "IAgentOrchestrator.Execute",
        "IMessageGenerator.GenerateMessage",
        "IMessageMiddleware.Generate",
        "IMessageService.GenerateMessage",
    ];

    [Fact]
    public void StreamingPairs_MatchKnownRoster()
    {
        var found = DiscoverPairs(typeof(IronHive.Abstractions.Agent.IAgent).Assembly);

        found.Should().BeEquivalentTo(
            KnownPairs,
            "a streaming/non-streaming pair is one operation exposed twice, and the two halves "
            + "drift silently. A pair that appears here without an equivalence test is exactly the "
            + "shape that passed CI three times in this ecosystem. Either add the pair to KnownPairs "
            + "together with a test asserting the two halves agree, or remove one of the halves. "
            + "See docs/CONVENTIONS.md section 5.");
    }

    // A pair is not always declared on one type. IAgentMiddleware/IStreamingAgentMiddleware split
    // the same operation across two interfaces, and that split is the more dangerous form: a
    // consumer may legally implement one and not the other, so the halves can disagree without
    // anything failing to compile. Same-type detection alone misses it -- which is how this test
    // found it, on its first run against real code.
    private static List<string> DiscoverPairs(Assembly assembly)
    {
        var buffered = new Dictionary<string, List<Type>>(StringComparer.Ordinal);
        var streaming = new Dictionary<string, List<Type>>(StringComparer.Ordinal);

        foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
        {
            foreach (var name in type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(m => m.Name))
            {
                if (!name.EndsWith(AsyncSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                // "Streaming" is not always a suffix: GenerateStreamingMessageAsync pairs with
                // GenerateMessageAsync. Matching only a trailing "StreamingAsync" missed both
                // message pairs -- the service that runs the tool loop and the generator every
                // provider implements -- while the roster reported four pairs as the whole set.
                if (name.Contains(StreamingToken, StringComparison.Ordinal))
                {
                    var stem = name.Replace(StreamingToken, string.Empty, StringComparison.Ordinal);
                    Add(streaming, stem[..^AsyncSuffix.Length], type);
                }
                else
                {
                    Add(buffered, name[..^AsyncSuffix.Length], type);
                }
            }
        }

        var pairs = new List<string>();

        foreach (var (stem, streamingDeclarers) in streaming)
        {
            if (stem.Length == 0 || !buffered.TryGetValue(stem, out var bufferedDeclarers))
            {
                continue;
            }

            foreach (var streamingType in streamingDeclarers)
            {
                // Same type first; otherwise the sibling whose name is this one with "Streaming"
                // taken out -- IStreamingAgentMiddleware pairs with IAgentMiddleware.
                var sameType = bufferedDeclarers.FirstOrDefault(t => t == streamingType);
                if (sameType is not null)
                {
                    pairs.Add($"{streamingType.Name}.{stem}");
                    continue;
                }

                var sibling = bufferedDeclarers.FirstOrDefault(
                    t => streamingType.Name.Replace("Streaming", string.Empty, StringComparison.Ordinal) == t.Name);

                if (sibling is not null)
                {
                    pairs.Add($"{sibling.Name}+{streamingType.Name}.{stem}");
                }
            }
        }

        return pairs;
    }

    private static void Add(Dictionary<string, List<Type>> map, string stem, Type type)
    {
        if (!map.TryGetValue(stem, out var list))
        {
            list = [];
            map[stem] = list;
        }

        list.Add(type);
    }
}
