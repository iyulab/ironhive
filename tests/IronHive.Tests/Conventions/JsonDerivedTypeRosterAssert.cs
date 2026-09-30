using System.Reflection;
using System.Text.Json.Serialization;
using AwesomeAssertions;

namespace IronHive.Tests.Conventions;

// Reusable guard for "a new variant of a closed set must not be absorbed silently": a new
// subtype has to be classified deliberately, not fall into a default branch. A [JsonDerivedType]-based discriminated
// union exposes its own known-subtype list via the attribute, so that list -- not a
// hand-maintained comment -- is the one place both truthful and complete to pin against.
public static class JsonDerivedTypeRosterAssert
{
    public static void MatchesKnownRoster(Type baseType, IEnumerable<Type> knownTypes, string becauseNewVariantFound)
    {
        var declared = baseType
            .GetCustomAttributes<JsonDerivedTypeAttribute>()
            .Select(a => a.DerivedType)
            .ToHashSet();

        declared.Should().BeEquivalentTo(knownTypes, becauseNewVariantFound);
    }
}
