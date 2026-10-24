using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// GPUStack's streaming tool-call parser fails on a parameter whose schema has a <c>type</c> list (<c>["string", "null"]</c>,
/// what a .NET optional nullable parameter exports), so the GPUStack route sends the schemas with <c>"null"</c> taken out of
/// every type list. Other routes send the schema as the tool declares it.
/// </summary>
public class GpuStackSchemaTypeListTests
{
    // The shape AIFunctionFactory exports for (string command, string? workingDirectory = null, Item[]? items = null).
    private const string Schema = """
        {"type":"object","properties":{"command":{"type":"string"},"workingDirectory":{"type":["string","null"],"default":null},
         "items":{"type":["array","null"],"items":{"type":["object","null"],"properties":{"text":{"type":["string","null"]},
         "status":{"anyOf":[{"type":["string","null"],"enum":["pending","done",null]}]}},"required":["text","status"]},"default":null}},
         "required":["command"]}
        """;

    private static MessageGenerationRequest Request(object parameters) => new()
    {
        Model = "qwen",
        Messages = [Message.User("hi")],
        Tools = new ToolCollection([new SchemaTool(parameters)]),
    };

    private static JsonNode SentParameters(ChatCompletionRequest request) =>
        JsonSerializer.SerializeToNode(request.Tools!.Single().Function.Parameters)!;

    [Fact]
    public void GpuStackRoute_StripsNullFromEveryTypeList()
    {
        var sent = SentParameters(ChatCompletionMessageGenerator.BuildRequest(Request(JsonNode.Parse(Schema)!), stripNullFromSchemaTypeLists: true));

        sent["properties"]!["workingDirectory"]!["type"]!.GetValue<string>().Should().Be("string");
        sent["properties"]!["items"]!["type"]!.GetValue<string>().Should().Be("array");
        var item = sent["properties"]!["items"]!["items"]!;
        item["type"]!.GetValue<string>().Should().Be("object");
        item["properties"]!["text"]!["type"]!.GetValue<string>().Should().Be("string");
        item["properties"]!["status"]!["anyOf"]![0]!["type"]!.GetValue<string>().Should().Be("string");
        sent.ToJsonString().Should().NotContain("[\"string\",\"null\"]").And.NotContain("\"null\"]");
    }

    [Fact]
    public void OtherRoutes_SendTheTypeListsAsDeclared()
    {
        var sent = SentParameters(ChatCompletionMessageGenerator.BuildRequest(Request(JsonNode.Parse(Schema)!)));

        sent["properties"]!["workingDirectory"]!["type"]!.ToJsonString().Should().Be("[\"string\",\"null\"]");
    }

    [Fact]
    public void Stripping_LeavesTheToolsOwnSchemaUnchanged()
    {
        var declared = JsonNode.Parse(Schema)!;
        var before = declared.ToJsonString();

        ChatCompletionMessageGenerator.BuildRequest(Request(declared), stripNullFromSchemaTypeLists: true);

        declared.ToJsonString().Should().Be(before);
    }

    [Fact]
    public void Stripping_ReadsAJsonElementSchemaToo()
    {
        var element = JsonDocument.Parse(Schema).RootElement;

        var sent = SentParameters(ChatCompletionMessageGenerator.BuildRequest(Request(element), stripNullFromSchemaTypeLists: true));

        sent["properties"]!["workingDirectory"]!["type"]!.GetValue<string>().Should().Be("string");
    }

    [Fact]
    public void GpuStackConfig_TurnsItOn_AndThePlainCompatibleConfigDoesNot()
    {
        new GpuStackConfig { BaseUrl = "http://localhost:1", ApiKey = "k" }.ToOpenAICompatible().StripNullFromSchemaTypeLists.Should().BeTrue();
        new OpenAICompatibleConfig { BaseUrl = "http://localhost:1" }.StripNullFromSchemaTypeLists.Should().BeFalse();
    }

    private sealed class SchemaTool(object parameters) : ITool
    {
        public string UniqueName => "ExecuteCommand";
        public string? Description => null;
        public object? Parameters => parameters;
        public bool RequiresApproval { get; set; }
        public Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(ToolOutput.Success(string.Empty));
    }
}
