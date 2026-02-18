using FluentAssertions;

namespace IronHive.Tests.Extensions;

public class StringExtensionsTests
{
    #region EnsureSuffix (string)

    [Fact]
    public void EnsureSuffix_String_AlreadyHasSuffix_ReturnsUnchanged()
    {
        "hello.txt".EnsureSuffix(".txt").Should().Be("hello.txt");
    }

    [Fact]
    public void EnsureSuffix_String_MissingSuffix_AppendsSuffix()
    {
        "hello".EnsureSuffix(".txt").Should().Be("hello.txt");
    }

    [Fact]
    public void EnsureSuffix_String_EmptyString_AppendsSuffix()
    {
        "".EnsureSuffix("/").Should().Be("/");
    }

    [Fact]
    public void EnsureSuffix_String_EmptySuffix_ReturnsUnchanged()
    {
        "hello".EnsureSuffix("").Should().Be("hello");
    }

    #endregion

    #region EnsureSuffix (char)

    [Fact]
    public void EnsureSuffix_Char_AlreadyHasSuffix_ReturnsUnchanged()
    {
        "path/".EnsureSuffix('/').Should().Be("path/");
    }

    [Fact]
    public void EnsureSuffix_Char_MissingSuffix_AppendsChar()
    {
        "path".EnsureSuffix('/').Should().Be("path/");
    }

    #endregion

    #region RemoveSuffix (string)

    [Fact]
    public void RemoveSuffix_String_HasSuffix_RemovesSuffix()
    {
        "hello.txt".RemoveSuffix(".txt").Should().Be("hello");
    }

    [Fact]
    public void RemoveSuffix_String_NoMatchingSuffix_ReturnsUnchanged()
    {
        "hello.csv".RemoveSuffix(".txt").Should().Be("hello.csv");
    }

    [Fact]
    public void RemoveSuffix_String_EmptySuffix_ReturnsUnchanged()
    {
        "hello".RemoveSuffix("").Should().Be("hello");
    }

    [Fact]
    public void RemoveSuffix_String_EntireStringIsSuffix_ReturnsEmpty()
    {
        ".txt".RemoveSuffix(".txt").Should().BeEmpty();
    }

    #endregion

    #region RemoveSuffix (char)

    [Fact]
    public void RemoveSuffix_Char_HasSuffix_RemovesChar()
    {
        "path/".RemoveSuffix('/').Should().Be("path");
    }

    [Fact]
    public void RemoveSuffix_Char_NoMatchingSuffix_ReturnsUnchanged()
    {
        "path".RemoveSuffix('/').Should().Be("path");
    }

    #endregion

    #region EnsurePrefix (string)

    [Fact]
    public void EnsurePrefix_String_AlreadyHasPrefix_ReturnsUnchanged()
    {
        "https://example.com".EnsurePrefix("https://").Should().Be("https://example.com");
    }

    [Fact]
    public void EnsurePrefix_String_MissingPrefix_PrependsPrefix()
    {
        "example.com".EnsurePrefix("https://").Should().Be("https://example.com");
    }

    [Fact]
    public void EnsurePrefix_String_EmptyString_PrependsPrefix()
    {
        "".EnsurePrefix("/").Should().Be("/");
    }

    #endregion

    #region EnsurePrefix (char)

    [Fact]
    public void EnsurePrefix_Char_AlreadyHasPrefix_ReturnsUnchanged()
    {
        "/path".EnsurePrefix('/').Should().Be("/path");
    }

    [Fact]
    public void EnsurePrefix_Char_MissingPrefix_PrependsChar()
    {
        "path".EnsurePrefix('/').Should().Be("/path");
    }

    #endregion

    #region RemovePrefix (string)

    [Fact]
    public void RemovePrefix_String_HasPrefix_RemovesPrefix()
    {
        "https://example.com".RemovePrefix("https://").Should().Be("example.com");
    }

    [Fact]
    public void RemovePrefix_String_NoMatchingPrefix_ReturnsUnchanged()
    {
        "http://example.com".RemovePrefix("https://").Should().Be("http://example.com");
    }

    [Fact]
    public void RemovePrefix_String_EmptyPrefix_ReturnsUnchanged()
    {
        "hello".RemovePrefix("").Should().Be("hello");
    }

    [Fact]
    public void RemovePrefix_String_EntireStringIsPrefix_ReturnsEmpty()
    {
        "hello".RemovePrefix("hello").Should().BeEmpty();
    }

    #endregion

    #region RemovePrefix (char)

    [Fact]
    public void RemovePrefix_Char_HasPrefix_RemovesChar()
    {
        "/path".RemovePrefix('/').Should().Be("path");
    }

    [Fact]
    public void RemovePrefix_Char_NoMatchingPrefix_ReturnsUnchanged()
    {
        "path".RemovePrefix('/').Should().Be("path");
    }

    #endregion

    #region OrDefault

    [Fact]
    public void OrDefault_NullString_ReturnsDefault()
    {
        string? str = null;
        str.OrDefault("fallback").Should().Be("fallback");
    }

    [Fact]
    public void OrDefault_EmptyString_ReturnsDefault()
    {
        "".OrDefault("fallback").Should().Be("fallback");
    }

    [Fact]
    public void OrDefault_WhitespaceOnly_ReturnsDefault()
    {
        "   ".OrDefault("fallback").Should().Be("fallback");
    }

    [Fact]
    public void OrDefault_NonEmptyString_ReturnsOriginal()
    {
        "hello".OrDefault("fallback").Should().Be("hello");
    }

    [Fact]
    public void OrDefault_StringWithWhitespace_ReturnsOriginal()
    {
        " hello ".OrDefault("fallback").Should().Be(" hello ");
    }

    #endregion
}
