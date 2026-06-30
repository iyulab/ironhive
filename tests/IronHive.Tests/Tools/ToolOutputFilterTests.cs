using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Tests.Tools;

public class ToolOutputFilterTests
{
    #region Basic Behavior

    [Fact]
    public void Filter_NullResult_ReturnsOriginal()
    {
        var filter = new ToolOutputFilter();
        var output = new ToolOutput(true, null);

        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    [Fact]
    public void Filter_EmptyResult_ReturnsOriginal()
    {
        var filter = new ToolOutputFilter();
        var output = new ToolOutput(true, "");

        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    [Fact]
    public void Filter_ShortResult_NoChange()
    {
        var filter = new ToolOutputFilter();
        var output = ToolOutput.Success("hello world");

        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    [Fact]
    public void Filter_PreservesIsSuccess()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            EnableWhitespaceNormalization = true
        });
        var output = ToolOutput.Failure("error\n\n\n\nmessage");

        var result = filter.Filter("test_tool", output);

        Assert.False(result.IsSuccess);
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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_TooFewElements_ReturnsOriginal()
    {
        var json = """[{"name": "foo"}, {"name": "bar"}]""";

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json, minElements: 3);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_NonJsonInput_ReturnsOriginal()
    {
        var text = "This is not JSON at all";

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(text);

        Assert.Equal(text, result);
    }

    [Fact]
    public void JsonToCsv_ArrayOfPrimitives_ReturnsOriginal()
    {
        var json = """[1, 2, 3, 4, 5]""";

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

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

        var result = ToolOutputFilter.TryConvertJsonArrayToCsv(json);

        Assert.Equal(json, result);
    }

    [Fact]
    public void JsonToCsv_RespectsMinElementsOption()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            EnableJsonToCsv = true,
            JsonToCsvMinElements = 5,
            EnableWhitespaceNormalization = false
        });

        // 3 elements — below custom threshold of 5, should NOT convert
        var json = """
            [
                {"name": "foo", "value": 1},
                {"name": "bar", "value": 2},
                {"name": "baz", "value": 3}
            ]
            """;

        var output = ToolOutput.Success(json);
        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    #endregion

    #region Whitespace Normalization

    [Fact]
    public void NormalizeWhitespace_CollapsesExcessiveNewlines()
    {
        var input = "line1\n\n\n\n\nline2";

        var result = ToolOutputFilter.NormalizeWhitespace(input);

        Assert.Equal("line1\n\nline2", result);
    }

    [Fact]
    public void NormalizeWhitespace_PreservesTwoNewlines()
    {
        var input = "line1\n\nline2";

        var result = ToolOutputFilter.NormalizeWhitespace(input);

        Assert.Equal("line1\n\nline2", result);
    }

    [Fact]
    public void NormalizeWhitespace_TrimsTrailingSpaces()
    {
        var input = "line1   \nline2\t\t\nline3";

        var result = ToolOutputFilter.NormalizeWhitespace(input);

        Assert.Equal("line1\nline2\nline3", result);
    }

    [Fact]
    public void NormalizeWhitespace_TrimsResult()
    {
        var input = "  \n\nhello world\n\n  ";

        var result = ToolOutputFilter.NormalizeWhitespace(input);

        Assert.Equal("hello world", result);
    }

    #endregion

    #region Truncation

    [Fact]
    public void Filter_OversizedResult_Truncates()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            MaxResultChars = 100,
            KeepHeadLines = 3,
            KeepTailLines = 2,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        });

        var lines = Enumerable.Range(1, 50).Select(i => $"Line {i}: some content here").ToArray();
        var output = ToolOutput.Success(string.Join('\n', lines));

        var result = filter.Filter("test_tool", output);

        Assert.Contains("Line 1:", result.Result);
        Assert.Contains("Line 2:", result.Result);
        Assert.Contains("Line 3:", result.Result);
        Assert.Contains("Line 49:", result.Result);
        Assert.Contains("Line 50:", result.Result);
        Assert.Contains("[... 45 lines omitted", result.Result);
        Assert.DoesNotContain("Line 25:", result.Result);
    }

    [Fact]
    public void Filter_OversizedSingleLine_CharacterTruncation()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            MaxResultChars = 100,
            KeepHeadLines = 5,
            KeepTailLines = 2,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        });

        // Single very long line (no newlines to split on)
        var output = ToolOutput.Success(new string('x', 200));

        var result = filter.Filter("test_tool", output);

        Assert.Contains("[... truncated", result.Result);
        Assert.True(result.Result!.Length < 200);
    }

    [Fact]
    public void Filter_UnderMaxChars_NoTruncation()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            MaxResultChars = 1000,
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false
        });

        var output = ToolOutput.Success("Short result");

        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    #endregion

    #region Combined Strategies

    [Fact]
    public void Filter_AllStrategiesApplied()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            EnableJsonToCsv = true,
            EnableWhitespaceNormalization = true,
            MaxResultChars = 50_000
        });

        // JSON array that should be converted to CSV + has trailing whitespace
        var json = """
            [
                {"id": 1, "name": "alpha", "status": "active"},
                {"id": 2, "name": "beta", "status": "inactive"},
                {"id": 3, "name": "gamma", "status": "active"}
            ]
            """;

        var output = ToolOutput.Success(json);
        var result = filter.Filter("test_tool", output);

        // Should be converted to CSV
        Assert.Contains("id,name,status", result.Result);
        Assert.Contains("1,alpha,active", result.Result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Filter_DisabledStrategies_NoChange()
    {
        var filter = new ToolOutputFilter(new ToolOutputFilterOptions
        {
            EnableJsonToCsv = false,
            EnableWhitespaceNormalization = false,
            MaxResultChars = int.MaxValue
        });

        var json = """
            [
                {"id": 1, "name": "alpha"},
                {"id": 2, "name": "beta"},
                {"id": 3, "name": "gamma"}
            ]
            """;

        var output = ToolOutput.Success(json);
        var result = filter.Filter("test_tool", output);

        Assert.Same(output, result);
    }

    #endregion

    #region CSV Escaping

    [Fact]
    public void EscapeCsvField_NoSpecialChars_Unchanged()
    {
        Assert.Equal("hello", ToolOutputFilter.EscapeCsvField("hello"));
    }

    [Fact]
    public void EscapeCsvField_Comma_Quoted()
    {
        Assert.Equal("\"hello, world\"", ToolOutputFilter.EscapeCsvField("hello, world"));
    }

    [Fact]
    public void EscapeCsvField_DoubleQuote_Escaped()
    {
        Assert.Equal("\"say \"\"hi\"\"\"", ToolOutputFilter.EscapeCsvField("say \"hi\""));
    }

    [Fact]
    public void EscapeCsvField_Newline_Quoted()
    {
        Assert.Equal("\"line1\nline2\"", ToolOutputFilter.EscapeCsvField("line1\nline2"));
    }

    #endregion

    #region Options Defaults

    [Fact]
    public void Options_DefaultValues()
    {
        var options = new ToolOutputFilterOptions();

        Assert.True(options.EnableJsonToCsv);
        Assert.Equal(3, options.JsonToCsvMinElements);
        Assert.True(options.EnableWhitespaceNormalization);
        Assert.Equal(50_000, options.MaxResultChars);
        Assert.Equal(100, options.KeepHeadLines);
        Assert.Equal(30, options.KeepTailLines);
    }

    #endregion
}
