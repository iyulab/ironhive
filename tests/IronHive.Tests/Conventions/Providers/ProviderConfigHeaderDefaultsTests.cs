using System.Reflection;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Conventions.Providers;

/// <summary>
/// A header dictionary on a provider config is usable the way its shape suggests: <c>config.Headers["X"] = "v"</c>
/// inside a configuration callback must not throw. A dictionary left null made that line fail at the first
/// request (the provider is built lazily, so the callback runs inside a user's turn) with a stack naming nobody's
/// configuration. The roster is read from the provider assemblies, so a new config type or a new header slot
/// is covered without editing this file.
/// </summary>
public class ProviderConfigHeaderDefaultsTests
{
    private static readonly string[] HeaderSlotNames = ["Headers", "ExtraHeaders"];

    private static readonly Assembly[] ProviderAssemblies =
    [
        typeof(OpenAIConfig).Assembly,
        typeof(OpenAICompatibleConfig).Assembly,
        typeof(AnthropicConfig).Assembly,
        typeof(GoogleAIConfig).Assembly,
    ];

    public static IEnumerable<object[]> HeaderSlots() =>
        ProviderAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Config", StringComparison.Ordinal)
                        && t.GetConstructor(Type.EmptyTypes) is not null)
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => HeaderSlotNames.Contains(p.Name)
                            && p.PropertyType == typeof(IDictionary<string, string>)
                            && p.CanRead && p.CanWrite)
                .Select(p => new object[] { $"{t.Name}.{p.Name}", t, p.Name }));

    [Fact]
    public void TheRosterSeesEveryKnownHeaderSlot()
    {
        // Positive control: a roster that matched nothing would pass every fact below.
        var slots = HeaderSlots().Select(s => (string)s[0]).ToArray();

        Assert.Contains("OpenAIConfig.Headers", slots);
        Assert.Contains("OpenAICompatibleConfig.Headers", slots);
        Assert.Contains("AnthropicConfig.Headers", slots);
        Assert.Contains("AnthropicConfig.ExtraHeaders", slots);
        Assert.Contains("GoogleAIConfig.Headers", slots);
        Assert.Contains("VertexAIConfig.Headers", slots);
        Assert.Contains("GpuStackConfig.Headers", slots);
    }

    [Theory]
    [MemberData(nameof(HeaderSlots))]
    public void AHeaderSlot_IsAnEmptyDictionaryByDefault_SoAnIndexerAssignmentWorks(string slot, Type configType, string propertyName)
    {
        var config = Activator.CreateInstance(configType)!;
        var property = configType.GetProperty(propertyName)!;

        var headers = (IDictionary<string, string>?)property.GetValue(config);

        Assert.True(headers is not null, $"{slot} is null by default; `config.{propertyName}[\"X\"] = \"v\"` would throw NullReferenceException.");
        Assert.Empty(headers);
        headers["X-Probe"] = "v"; // must not throw
        Assert.Equal("v", ((IDictionary<string, string>)property.GetValue(config)!)["X-Probe"]);
    }
}
