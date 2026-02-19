using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace IronHive.Tests.Extensions;

public class ObjectExtensionsTests
{
    #region ConvertTo<T>

    [Fact]
    public void ConvertTo_Null_ReturnsNull()
    {
        object? obj = null;

        obj.ConvertTo<string>().Should().BeNull();
    }

    [Fact]
    public void ConvertTo_AlreadyTargetType_ReturnsSameObject()
    {
        object obj = "hello";

        var result = obj.ConvertTo<string>();

        result.Should().Be("hello");
    }

    [Fact]
    public void ConvertTo_StringToInt_Deserializes()
    {
        object obj = "42";

        var result = obj.ConvertTo<int>();

        result.Should().Be(42);
    }

    [Fact]
    public void ConvertTo_StringToObject_Deserializes()
    {
        object obj = """{"Name":"Alice","Age":30}""";

        var result = obj.ConvertTo<TestPerson>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void ConvertTo_JsonElement_Deserializes()
    {
        var doc = JsonDocument.Parse("""{"Name":"Bob","Age":25}""");
        object obj = doc.RootElement;

        var result = obj.ConvertTo<TestPerson>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Bob");
        result.Age.Should().Be(25);
    }

    [Fact]
    public void ConvertTo_JsonDocument_Deserializes()
    {
        using var doc = JsonDocument.Parse("""{"Name":"Carol","Age":35}""");

        var result = ((object)doc).ConvertTo<TestPerson>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Carol");
    }

    [Fact]
    public void ConvertTo_JsonNode_Deserializes()
    {
        var node = JsonNode.Parse("""{"Name":"Dave","Age":40}""");

        var result = ((object?)node).ConvertTo<TestPerson>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Dave");
    }

    [Fact]
    public void ConvertTo_AnyObject_ViaJsonRoundTrip()
    {
        var source = new { Name = "Eve", Age = 28 };

        var result = ((object)source).ConvertTo<TestPerson>();

        result.Should().NotBeNull();
        result!.Name.Should().Be("Eve");
        result.Age.Should().Be(28);
    }

    [Fact]
    public void ConvertTo_InvalidConversion_ReturnsNull()
    {
        object obj = "not-a-valid-json-object";

        var result = obj.ConvertTo<TestPerson>();

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertTo_InvalidStringToValueType_ReturnsDefault()
    {
        object obj = "not-a-number";

        var result = obj.ConvertTo<int>();

        result.Should().Be(0);
    }

    [Fact]
    public void ConvertTo_NullToValueType_ReturnsDefault()
    {
        object? obj = null;

        var result = obj.ConvertTo<int>();

        result.Should().Be(0);
    }

    [Fact]
    public void ConvertTo_InvalidStringToBool_ReturnsDefault()
    {
        object obj = "not-a-bool";

        var result = obj.ConvertTo<bool>();

        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertTo_AssignableType_ReturnsSameObject()
    {
        var list = new List<string> { "a", "b" };

        var result = ((object)list).ConvertTo<IEnumerable<string>>();

        result.Should().BeSameAs(list);
    }

    #endregion

    #region TryConvertTo<T>

    [Fact]
    public void TryConvertTo_Success_ReturnsTrueAndValue()
    {
        object obj = "42";

        var success = obj.TryConvertTo<int>(out var value);

        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryConvertTo_Null_ReturnsFalse()
    {
        object? obj = null;

        var success = obj.TryConvertTo<string>(out var value);

        success.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryConvertTo_InvalidStringToValueType_ReturnsFalse()
    {
        object obj = "not-a-number";

        var success = obj.TryConvertTo<int>(out var value);

        success.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void TryConvertTo_NullToValueType_ReturnsFalse()
    {
        object? obj = null;

        var success = obj.TryConvertTo<int>(out var value);

        success.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void TryConvertTo_NonGeneric_Success()
    {
        object obj = """{"Name":"Frank","Age":50}""";

        var success = obj.TryConvertTo(typeof(TestPerson), out var value);

        success.Should().BeTrue();
        value.Should().BeOfType<TestPerson>();
        ((TestPerson)value!).Name.Should().Be("Frank");
    }

    #endregion

    #region Clone

    [Fact]
    public void Clone_Null_ReturnsNull()
    {
        TestPerson? obj = null;

        var clone = obj.Clone();

        clone.Should().BeNull();
    }

    [Fact]
    public void Clone_ValueType_ReturnsSameValue()
    {
        int value = 42;

        var clone = value.Clone();

        clone.Should().Be(42);
    }

    [Fact]
    public void Clone_String_ReturnsSameInstance()
    {
        string str = "hello";

        var clone = str.Clone();

        clone.Should().Be("hello");
    }

    [Fact]
    public void Clone_ComplexObject_CreatesDeepCopy()
    {
        var original = new TestPerson { Name = "Grace", Age = 45 };

        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Name.Should().Be("Grace");
        clone.Age.Should().Be(45);
    }

    [Fact]
    public void Clone_DeepCopy_MutationDoesNotAffectOriginal()
    {
        var original = new TestPerson { Name = "Hank", Age = 60 };

        var clone = original.Clone();
        clone.Name = "Modified";

        original.Name.Should().Be("Hank");
    }

    [Fact]
    public void Clone_List_CreatesDeepCopy()
    {
        var original = new List<string> { "a", "b", "c" };

        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original);
        clone.Add("d");
        original.Should().HaveCount(3);
    }

    #endregion

    private sealed class TestPerson
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
}
