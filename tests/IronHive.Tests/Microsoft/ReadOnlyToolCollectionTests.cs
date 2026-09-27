using AwesomeAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Extensions.AI;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Microsoft;

/// <summary>
/// The adapter's per-request tool set. Before IronHive.Extensions.AI existed the adapter built Core's mutable
/// <c>ToolCollection</c>, which is why the bridge could not leave IronHive.Core. Providers only read the set
/// (<c>OpenAIMessageGenerator</c> calls <see cref="IToolCollection.FilterBy"/> for a multi-tool choice), so a read-only
/// snapshot is the honest type — these facts pin what providers rely on and that nothing can change it mid-request.
/// </summary>
public class ReadOnlyToolCollectionTests
{
    private static AIToolAdapter Tool(string name) => new AIToolAdapter(AIFunctionFactory.Create(() => name, name: name));

    [Fact]
    public void Lookup_IsCaseInsensitive_AndEnumerationKeepsTheCallersOrder()
    {
        var tools = new ReadOnlyToolCollection([Tool("search"), Tool("get_weather"), Tool("add")]);

        tools.Select(t => t.UniqueName).Should().Equal("search", "get_weather", "add");
        tools.ContainsKey("GET_WEATHER").Should().BeTrue();
        tools.TryGet("Search", out var found).Should().BeTrue();
        found!.UniqueName.Should().Be("search");
        tools.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void FilterBy_ReturnsTheNamedSubset_IgnoringUnknownNames()
    {
        var tools = new ReadOnlyToolCollection([Tool("search"), Tool("get_weather"), Tool("add")]);

        var filtered = tools.FilterBy(["ADD", "search", "missing"]);

        filtered.Select(t => t.UniqueName).Should().Equal("search", "add");
        filtered.IsReadOnly.Should().BeTrue();
        tools.Should().HaveCount(3, "filtering must not change the source");
    }

    [Fact]
    public void DuplicateNames_AreRejected_AsTheMutableCollectionDid()
    {
        var act = () => new ReadOnlyToolCollection([Tool("search"), Tool("SEARCH")]);

        act.Should().Throw<ArgumentException>().WithMessage("*same key*search*");
    }

    [Fact]
    public void EveryMutation_Throws()
    {
        var tools = new ReadOnlyToolCollection([Tool("search")]);
        var extra = Tool("add");

        new Action[]
        {
            () => tools.Add(extra), () => tools.AddRange([extra]), () => tools.Set(extra), () => tools.SetRange([extra]),
            () => tools.Remove("search"), () => tools.Remove(extra), () => tools.RemoveAll(), () => tools.Clear(),
        }.Should().AllSatisfy(a => a.Should().Throw<NotSupportedException>());
        tools.Should().ContainSingle();
    }
}
