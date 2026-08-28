using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Tests.Conventions;
using Xunit;

namespace IronHive.Tests;

// C# has no compiler-checked exhaustiveness for switches over a non-sealed class hierarchy
// (verified empirically: even a switch expression listing every currently-known MessageContent
// subtype fails CS8509 without a discard arm, and a discard arm permanently silences the
// diagnostic for any type added later -- see BD-20260828-04's issue draft Resolution for the
// build-failure evidence). MessageContent is public and unsealed across an assembly boundary
// (IronHive.Abstractions -> the 4 IronHive.Providers.* projects), so CS8509 can never provide
// real protection here.
//
// This test pins the known-subtype roster instead, at the one place both truthful and complete:
// MessageContent's own [JsonDerivedType] declarations (the same list JSON polymorphism already
// depends on being exhaustive). Adding a new content type breaks this test immediately in CI --
// the failure message is the checklist: decide how each of the 4 IronHive.Providers.*
// MessageGenerators (Anthropic/OpenAI/OpenAI.Compatible/GoogleAI) should handle it, then update
// the roster below. This is CI-time, not compile-time, but it is exhaustive where CS8509 cannot
// be; a runtime `NotImplementedException` deep in a provider is otherwise the only signal.
public class MessageContentRosterTests
{
    private static readonly Type[] KnownContentTypes =
    [
        typeof(TextMessageContent),
        typeof(ImageMessageContent),
        typeof(ToolMessageContent),
        typeof(ThinkingMessageContent),
    ];

    [Fact]
    public void MessageContent_JsonDerivedTypes_MatchesKnownRoster()
    {
        JsonDerivedTypeRosterAssert.MatchesKnownRoster(
            typeof(MessageContent),
            KnownContentTypes,
            "a new MessageContent subtype was added -- decide how each of the 4 " +
            "IronHive.Providers.* MessageGenerators (Anthropic/OpenAI/OpenAI.Compatible/GoogleAI) " +
            "handles it, then add it to this test's KnownContentTypes roster");
    }
}
