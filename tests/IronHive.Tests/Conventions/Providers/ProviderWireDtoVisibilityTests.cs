using AwesomeAssertions;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// A provider's wire payloads are serialization detail, not API: public, every fix to the wire shape is a
/// breaking change, and names like <c>ChatMessage</c> or <c>ChatResponseFormat</c> collide with
/// Microsoft.Extensions.AI for a consumer that imports both namespaces.
/// </summary>
public class ProviderWireDtoVisibilityTests
{
    [Fact]
    public void OpenAICompatible_ExportsNoTypeNamedLikeAMicrosoftExtensionsAIType()
    {
        var meaiNames = typeof(global::Microsoft.Extensions.AI.ChatMessage).Assembly.GetExportedTypes()
            .Select(t => t.Name)
            .ToHashSet(StringComparer.Ordinal);

        var collisions = typeof(OpenAICompatibleConfig).Assembly.GetExportedTypes()
            .Where(t => meaiNames.Contains(t.Name))
            .Select(t => t.FullName)
            .ToList();

        collisions.Should().BeEmpty("a consumer using both namespaces would get ambiguous-reference errors");
    }

    [Fact]
    public void OpenAICompatible_WirePayloadsAreNotExported()
    {
        var exported = typeof(OpenAICompatibleConfig).Assembly.GetExportedTypes();

        exported.Where(t => t.Name.EndsWith("Request", StringComparison.Ordinal) || t.Name.EndsWith("Response", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .Should().BeEmpty("request/response payloads are the wire shape, which must stay free to change");
    }
}
