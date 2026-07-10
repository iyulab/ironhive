using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

public class TextCompactorTests
{
    #region Basic Behavior

    [Fact]
    public void Compact_NullResult_ReturnsOriginal()
    {
        string? text = null;

        var result = TextCompactor.Compact(text!);

        Assert.Same(text, result);
    }

    [Fact]
    public void Compact_EmptyResult_ReturnsOriginal()
    {
        var text = "";

        var result = TextCompactor.Compact(text);

        Assert.Same(text, result);
    }

    [Fact]
    public void Compact_ShortResult_NoChange()
    {
        var text = "hello world";

        var result = TextCompactor.Compact(text);

        Assert.Equal(text, result);
    }

    #endregion

    #region JSON to CSV Conversion

    [Fact]
    public void JsonToCsv_ArrayOfFlatObjects_ConvertsToCsv()
    {
        var json = """
            [
                {"name": "foo", "size": 123, "active": true},
                {"name": "bar", "size": 456, "active": false},
                {"name": "baz", "size": 789, "active": true}
            ]
            """;

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Contains("name,size,active", result);
        Assert.Contains("foo,123,true", result);
        Assert.Contains("bar,456,false", result);
        Assert.Contains("baz,789,true", result);
        // CSV should be shorter than JSON
        Assert.True(result.Length < json.Length);
    }

    [Fact]
    public void JsonToCsv_NotArray_ReturnsOriginal()
    {
        var json = """{"name": "foo", "size": 123}""";

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_NestedObjects_ReturnsOriginal()
    {
        var json = """
            [
                {"name": "foo", "meta": {"key": "value"}},
                {"name": "bar", "meta": {"key": "value2"}},
                {"name": "baz", "meta": {"key": "value3"}}
            ]
            """;

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_TooFewElements_ReturnsOriginal()
    {
        var json = """[{"name": "foo"}, {"name": "bar"}]""";

        var result = TextCompactor.TryConvertJsonArrayToCsv(json, minElements: 3);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_NonJsonInput_ReturnsOriginal()
    {
        var text = "This is not JSON at all";

        var result = TextCompactor.TryConvertJsonArrayToCsv(text);

        Assert.Equal(text, result);
    }

    [Fact]
    public void JsonToCsv_ArrayOfPrimitives_ReturnsOriginal()
    {
        var json = """[1, 2, 3, 4, 5]""";

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_StringWithComma_EscapesProperly()
    {
        var json = """
            [
                {"name": "foo, bar", "value": 1},
                {"name": "baz", "value": 2},
                {"name": "qux", "value": 3}
            ]
            """;

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Contains("\"foo, bar\"", result);
    }

    [Fact]
    public void JsonToCsv_NullValues_HandledGracefully()
    {
        var json = """
            [
                {"name": "foo", "value": null},
                {"name": "bar", "value": 42},
                {"name": "baz", "value": null}
            ]
            """;

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Contains("name,value", result);
        Assert.Contains("foo,", result);
        Assert.Contains("bar,42", result);
    }

    [Fact]
    public void JsonToCsv_ArraysInValues_ReturnsOriginal()
    {
        var json = """
            [
                {"name": "foo", "tags": ["a", "b"]},
                {"name": "bar", "tags": ["c"]},
                {"name": "baz", "tags": []}
            ]
            """;

        var result = TextCompactor.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_RespectsMinElementsOption()
    {
        var options = new TextCompactorOptions
        {
            EnableJsonToCsv = true,
            JsonToCsvMinElements = 5,
            EnableWhitespaceNormalization = false
        };

        // 3 elements — below custom threshold of 5, should NOT convert
        var json = """
            [
                {"name": "foo", "value": 1},
                {"name": "bar", "value": 2},
                {"name": "baz", "value": 3}
            ]
            """;

        var result = TextCompactor.Compact(json, options);

        Assert.Equal(json, result);
    }

    #endregion

    #region Whitespace Normalization

    [Fact]
    public void NormalizeWhitespace_CollapsesExcessiveNewlines()
    {
        var input = "line1\n\n\n\n\nline2";

        var result = TextCompactor.NormalizeWhitespace(input);

        Assert.Equal("line1\n\nline2", result);
    }

    [Fact]
    public void NormalizeWhitespace_PreservesTwoNewlines()
    {
        var input = "line1\n\nline2";

        var result = TextCompactor.NormalizeWhitespace(input);

        Assert.Equal("line1\n\nline2", result);
    }

    [Fact]
    public void NormalizeWhitespace_TrimsTrailingSpaces()
    {
        var input = "line1   \nline2\t\t\nline3";

        var result = TextCompactor.NormalizeWhitespace(input);

        Assert.Equal("line1\nline2\nline3", result);
    }

    [Fact]
    public void NormalizeWhitespace_TrimsResult()
    {
        var input = "  \n\nhello world\n\n  ";

        var result = TextCompactor.NormalizeWhitespace(input);

        Assert.Equal("hello world", result);
    }

    #endregion

    #region Truncation

    [Fact]
    public void Compact_OversizedResult_Truncates()
    {
        var options = new TextCompactorOptions
        {
            MaxResultChars = 100,
            KeepHeadLines = 3,
            KeepTailLines = 2,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        };

        var lines = Enumerable.Range(1, 50).Select(i => $"Line {i}: some content here").ToArray();
        var text = string.Join('\n', lines);

        var result = TextCompactor.Compact(text, options);

        Assert.Contains("Line 1:", result);
        Assert.Contains("Line 2:", result);
        Assert.Contains("Line 3:", result);
        Assert.Contains("Line 49:", result);
        Assert.Contains("Line 50:", result);
        Assert.Contains("[... 45 lines omitted", result);
        Assert.DoesNotContain("Line 25:", result);
    }

    [Fact]
    public void Compact_OversizedSingleLine_CharacterTruncation()
    {
        var options = new TextCompactorOptions
        {
            MaxResultChars = 100,
            KeepHeadLines = 5,
            KeepTailLines = 2,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        };

        // Single very long line (no newlines to split on)
        var text = new string('x', 200);

        var result = TextCompactor.Compact(text, options);

        Assert.Contains("[... truncated", result);
        Assert.True(result.Length < 200);
    }

    [Fact]
    public void Compact_UnderMaxChars_NoTruncation()
    {
        var options = new TextCompactorOptions
        {
            MaxResultChars = 1000,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        };

        var text = "Short result";

        var result = TextCompactor.Compact(text, options);

        Assert.Equal(text, result);
    }

    #endregion

    #region Combined Strategies

    [Fact]
    public void Compact_AllStrategiesApplied()
    {
        var options = new TextCompactorOptions
        {
            EnableJsonToCsv = true,
            EnableWhitespaceNormalization = true,
            MaxResultChars = 50_000
        };

        // JSON array that should be converted to CSV
        var json = """
            [
                {"id": 1, "name": "alpha", "status": "active"},
                {"id": 2, "name": "beta", "status": "inactive"},
                {"id": 3, "name": "gamma", "status": "active"}
            ]
            """;

        var result = TextCompactor.Compact(json, options);

        // Should be converted to CSV
        Assert.Contains("id,name,status", result);
        Assert.Contains("1,alpha,active", result);
    }

    [Fact]
    public void Compact_DisabledStrategies_NoChange()
    {
        var options = new TextCompactorOptions
        {
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false,
            MaxResultChars = int.MaxValue
        };

        var json = """
            [
                {"id": 1, "name": "alpha"},
                {"id": 2, "name": "beta"},
                {"id": 3, "name": "gamma"}
            ]
            """;

        var result = TextCompactor.Compact(json, options);

        Assert.Equal(json, result);
    }

    #endregion

    #region CSV Escaping

    [Fact]
    public void EscapeCsvField_NoSpecialChars_Unchanged()
    {
        Assert.Equal("hello", TextCompactor.EscapeCsvField("hello"));
    }

    [Fact]
    public void EscapeCsvField_Comma_Quoted()
    {
        Assert.Equal("\"hello, world\"", TextCompactor.EscapeCsvField("hello, world"));
    }

    [Fact]
    public void EscapeCsvField_DoubleQuote_Escaped()
    {
        Assert.Equal("\"say \"\"hi\"\"\"", TextCompactor.EscapeCsvField("say \"hi\""));
    }

    [Fact]
    public void EscapeCsvField_Newline_Quoted()
    {
        Assert.Equal("\"line1\nline2\"", TextCompactor.EscapeCsvField("line1\nline2"));
    }

    #endregion

    #region Options Defaults

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new TextCompactorOptions();

        Assert.True(options.EnableJsonToCsv);
        Assert.Equal(3, options.JsonToCsvMinElements);
        Assert.True(options.EnableWhitespaceNormalization);
        Assert.Equal(50_000, options.MaxResultChars);
        Assert.Equal(100, options.KeepHeadLines);
        Assert.Equal(30, options.KeepTailLines);
    }

    #endregion
}
