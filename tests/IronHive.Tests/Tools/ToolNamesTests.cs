using System.Text.RegularExpressions;
using AwesomeAssertions;
using IronHive.Abstractions.Tools;
using IronHive.Plugins.OpenAPI;

namespace IronHive.Tests.Tools;

/// <summary>
/// The name a model is offered for a tool must match what function-calling providers accept, <c>[a-zA-Z0-9_-]{1,64}</c>;
/// a name that already does is never changed.
/// </summary>
public partial class ToolNamesTests
{
    [GeneratedRegex("^[a-zA-Z0-9_-]{1,64}$")]
    private static partial Regex ProviderPattern();

    [Theory]
    [InlineData("get_weather")]
    [InlineData("func_custom_name")]
    [InlineData("mcp_files_read-file")]
    [InlineData("A")]
    public void A_name_the_providers_accept_is_unchanged(string name)
        => ToolNames.ProviderSafe(name).Should().BeSameAs(name);

    [Fact]
    public void A_name_of_exactly_64_safe_characters_is_unchanged()
    {
        var name = new string('a', 64);

        ToolNames.ProviderSafe(name).Should().Be(name);
    }

    [Fact]
    public void A_dotted_name_gets_its_safe_characters_and_a_stable_hash_of_the_whole_name()
    {
        // Golden values: the agent layer's ToolNameAliasChatClient.AliasFor("message.send") gives the same string, so a
        // name made safe here is left alone further up.
        ToolNames.ProviderSafe("message.send").Should().Be("message_send_c0cd13f497");
        ToolNames.ProviderSafe("mcp_srv_message.send").Should().Be("mcp_srv_message_send_ad911ca6aa");
        ToolNames.ProviderSafe("openapi_pets_GET /a.b").Should().Be("openapi_pets_GET__a_b_79ac9573d2");
    }

    [Fact]
    public void An_over_long_name_is_cut_to_64_characters_and_stays_distinguishable()
    {
        var first = new string('a', 80) + "x";
        var second = new string('a', 80) + "y";

        var safeFirst = ToolNames.ProviderSafe(first);
        var safeSecond = ToolNames.ProviderSafe(second);

        safeFirst.Should().HaveLength(64).And.MatchRegex(ProviderPattern().ToString());
        safeFirst.Should().NotBe(safeSecond, "the hash covers the whole name, not the part that was kept");
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("한글.도구")]
    public void Any_other_name_becomes_one_the_providers_accept(string name)
        => ToolNames.ProviderSafe(name).Should().MatchRegex(ProviderPattern().ToString());

    [Fact]
    public void It_is_a_pure_function_of_the_name()
        => ToolNames.ProviderSafe("fs/read").Should().Be(ToolNames.ProviderSafe("fs/read"));

    [Fact]
    public void A_null_name_is_refused()
    {
        var act = () => ToolNames.ProviderSafe(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void An_OpenAPI_operation_with_dots_and_slashes_is_offered_under_a_name_the_providers_accept()
    {
        using var http = new HttpClient();
        var tool = new OpenApiTool(http)
        {
            ClientName = "pets",
            OperationId = "GET /pets.v2/{id}",
            BaseUris = [new Uri("https://pets.test")],
            Method = HttpMethod.Get,
            Path = "/pets/{id}",
            Properties = new OpenApiProperties(),
        };

        tool.UniqueName.Should().MatchRegex(ProviderPattern().ToString()).And.StartWith("openapi_pets_GET");
    }

    [Fact]
    public void An_OpenAPI_operation_with_a_plain_id_keeps_its_name()
    {
        using var http = new HttpClient();
        var tool = new OpenApiTool(http)
        {
            ClientName = "pets",
            OperationId = "listPets",
            BaseUris = [new Uri("https://pets.test")],
            Method = HttpMethod.Get,
            Path = "/pets",
            Properties = new OpenApiProperties(),
        };

        tool.UniqueName.Should().Be("openapi_pets_listPets");
    }
}
