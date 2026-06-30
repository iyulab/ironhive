using FluentAssertions;
using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

// MessagePack ContractlessResolver requires public types for dynamic serialization
public sealed class SerializerTestPayload
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public List<string> Tags { get; set; } = [];
}

public sealed class SerializerTestContainer
{
    public string Id { get; set; } = "";
    public SerializerTestPayload? Payload { get; set; }
}

public class MessageSerializerTests
{
    // --- Serialize null ---

    [Fact]
    public void Serialize_Null_ReturnsNull()
    {
        var result = MessageSerializer.Serialize<string>(null!);

        result.Should().BeNull();
    }

    // --- Deserialize null / empty ---

    [Fact]
    public void Deserialize_NullBuffer_ReturnsDefault()
    {
        var result = MessageSerializer.Deserialize<string>(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyBuffer_ReturnsDefault()
    {
        var result = MessageSerializer.Deserialize<string>([]);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyBuffer_ReturnsDefaultForValueType()
    {
        var result = MessageSerializer.Deserialize<int>([]);

        result.Should().Be(0);
    }

    // --- Round-trip: primitives ---

    [Fact]
    public void RoundTrip_String_PreservesValue()
    {
        var original = "Hello, MessagePack!";
        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<string>(bytes!);

        result.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Integer_PreservesValue()
    {
        var original = 42;
        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<int>(bytes!);

        result.Should().Be(original);
    }

    [Fact]
    public void RoundTrip_Boolean_PreservesValue()
    {
        var bytes = MessageSerializer.Serialize(true);
        var result = MessageSerializer.Deserialize<bool>(bytes!);

        result.Should().BeTrue();
    }

    // --- Round-trip: complex objects ---

    [Fact]
    public void RoundTrip_ComplexObject_PreservesProperties()
    {
        var original = new SerializerTestPayload
        {
            Name = "test",
            Value = 123,
            Tags = ["a", "b", "c"]
        };

        var bytes = MessageSerializer.Serialize(original);
        bytes.Should().NotBeNull();
        bytes!.Length.Should().BeGreaterThan(0);

        var result = MessageSerializer.Deserialize<SerializerTestPayload>(bytes);

        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
        result.Value.Should().Be(123);
        result.Tags.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public void RoundTrip_NestedObject_PreservesStructure()
    {
        var original = new SerializerTestContainer
        {
            Id = "container-1",
            Payload = new SerializerTestPayload
            {
                Name = "nested",
                Value = 999,
                Tags = ["x"]
            }
        };

        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<SerializerTestContainer>(bytes!);

        result.Should().NotBeNull();
        result!.Id.Should().Be("container-1");
        result.Payload.Should().NotBeNull();
        result.Payload!.Name.Should().Be("nested");
        result.Payload.Value.Should().Be(999);
    }

    // --- Round-trip: collections ---

    [Fact]
    public void RoundTrip_List_PreservesElements()
    {
        var original = new List<int> { 1, 2, 3, 4, 5 };
        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<List<int>>(bytes!);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_Dictionary_PreservesEntries()
    {
        var original = new Dictionary<string, int>
        {
            ["alpha"] = 1,
            ["beta"] = 2,
            ["gamma"] = 3
        };

        var bytes = MessageSerializer.Serialize(original);
        var result = MessageSerializer.Deserialize<Dictionary<string, int>>(bytes!);

        result.Should().BeEquivalentTo(original);
    }

    // --- Serialized data uses LZ4 compression ---

    [Fact]
    public void Serialize_ProducesNonEmptyBytes()
    {
        var original = "A string to serialize";
        var bytes = MessageSerializer.Serialize(original);

        bytes.Should().NotBeNull();
        bytes!.Length.Should().BeGreaterThan(0);
    }
}
