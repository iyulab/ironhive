using System.Reflection;
using Iyu.Conventions.Testing;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// Every public option in this library is read by the library. An option nothing reads is a promise it does not keep:
/// a caller sets it, and nothing changes and nothing is reported. The roster fails both ways - a new unread option,
/// and a listed one that has since been wired - so each change is recorded on purpose.
/// </summary>
public class OptionsReachabilityRosterTests
{
    // Every assembly this repository ships: an option declared in one and read in another only counts as read
    // when both are scanned.
    private static readonly Assembly[] Libraries =
    [
        Assembly.Load("IronHive.Abstractions"),
        Assembly.Load("IronHive.Core"),
        Assembly.Load("IronHive.Plugins.MCP"),
        Assembly.Load("IronHive.Plugins.OpenAPI"),
        Assembly.Load("IronHive.Providers.Anthropic"),
        Assembly.Load("IronHive.Providers.GoogleAI"),
        Assembly.Load("IronHive.Providers.OpenAI"),
        Assembly.Load("IronHive.Providers.OpenAI.Compatible"),
        Assembly.Load("IronHive.Storages.Amazon"),
        Assembly.Load("IronHive.Storages.Azure"),
        Assembly.Load("IronHive.Storages.Qdrant"),
        Assembly.Load("IronHive.Storages.RabbitMQ"),
    ];

    /// <summary>
    /// Options accepted as unread today, each checked by hand (2026-09-20). Shrink this list; never grow it silently.
    /// </summary>
    private static readonly Dictionary<string, string[]> KnownUnread = new()
    {
        // A one-member enum: there is nothing to choose between yet, so nothing reads the choice.
        ["IronHive.Abstractions.Memory.SearchOptions"] = ["Mode"],

        // Read, but only by the type's own conversion methods (ResolveBaseUrl / ResolveApiKey feeding
        // ToOpenAICompatibleConfig), and the scan does not count a type reading itself. The provider calls
        // those methods, so both values do reach the wire.
        ["IronHive.Providers.OpenAI.Compatible.GpuStack.GpuStackConfig"] = ["ApiKey", "BaseUrl"],

        // Computed, get-only status for the consumer to read (is a key present, is an endpoint usable).
        // They are outputs of the configuration, not inputs to the library.
        ["IronHive.Providers.OpenAI.Compatible.OpenAICompatibleConfig"] = ["IsConfigured", "IsUsable"],
    };

    [Fact]
    public void EveryPublicOption_IsRead() =>
        OptionsReachability.Scan(Libraries, OptionsTypes.NamedWith("Options", "Config"))
            .ShouldMatchRoster(KnownUnread);
}
