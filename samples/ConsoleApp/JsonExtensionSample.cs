using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.JsonConverters;

namespace ConsoleApp;

/// <summary>
/// ExtraBody deep merge/split 동작을 확인하는 샘플입니다.
/// </summary>
public static class JsonExtensionSample
{
    public static void Run()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new ExtraBodyJsonConverterFactory());

        Console.WriteLine("--- 1. Serialize: ExtraBody deep merge ---");
        var request = new SampleRequest
        {
            Name = "john",
            Age = 30,
            Address = new SampleAddress { City = "Seoul" },
            ExtraBody = new JsonObject
            {
                // address.city는 유지하면서 zipcode가 병합됨
                ["address"] = new JsonObject { ["zipcode"] = "12345" },
                ["extra_field"] = true,
            }
        };
        Console.WriteLine(JsonSerializer.Serialize(request, options));

        Console.WriteLine();
        Console.WriteLine("--- 2. Deserialize: 미매핑 속성 → ExtraBody 수집 ---");
        var rawJson = """
        {
            "name": "jane",
            "age": 25,
            "unknown_field": "hello",
            "nested": { "a": 1, "b": 2 }
        }
        """;
        var deserialized = JsonSerializer.Deserialize<SampleRequest>(rawJson, options)!;
        Console.WriteLine($"Name: {deserialized.Name}");
        Console.WriteLine($"Age: {deserialized.Age}");
        Console.WriteLine($"ExtraBody: {JsonSerializer.Serialize(deserialized.ExtraBody, options)}");

        Console.WriteLine();
        Console.WriteLine("--- 3. Round-trip (deep merge → deep split) ---");
        var serialized = JsonSerializer.Serialize(request, options);
        var roundTrip = JsonSerializer.Deserialize<SampleRequest>(serialized, options)!;
        Console.WriteLine($"Name: {roundTrip.Name}, Age: {roundTrip.Age}");
        Console.WriteLine($"Address.City: {roundTrip.Address?.City}");
        Console.WriteLine($"ExtraBody: {JsonSerializer.Serialize(roundTrip.ExtraBody, options)}");
    }
}

public class SampleRequest : OpenAIPayloadBase
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("address")]
    public SampleAddress? Address { get; set; }
}

public class SampleAddress
{
    [JsonPropertyName("city")]
    public string? City { get; set; }
}
