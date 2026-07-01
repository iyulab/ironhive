using System.Text;
using FluentAssertions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files.Parsers;

namespace IronHive.Tests.Files;

public class ImageParserTests
{
    private readonly ImageParser _parser = new();

    [Theory]
    [InlineData("photo.png",  null,         true)]
    [InlineData("photo.jpg",  null,         true)]
    [InlineData("photo.jpeg", null,         true)]
    [InlineData("photo.gif",  null,         true)]
    [InlineData("photo.webp", null,         true)]
    [InlineData("photo.PNG",  null,         true)]
    [InlineData("file",       "image/png",  true)]
    [InlineData("file",       "image/webp", true)]
    [InlineData("photo.svg",  null,         false)]
    [InlineData("doc.pdf",    null,         false)]
    [InlineData("file",       "text/plain", false)]
    public void CanParse_ReturnsExpected(string fileName, string? mimeType, bool expected)
    {
        _parser.CanParse(fileName, mimeType).Should().Be(expected);
    }

    [Theory]
    [InlineData(".png",  "image/png")]
    [InlineData(".jpg",  "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".gif",  "image/gif")]
    [InlineData(".webp", "image/webp")]
    public async Task ParseAsync_ReturnsImageBlock_WithCorrectMimeType(string ext, string expectedMime)
    {
        var bytes = new byte[] { 1, 2, 3 };
        using var stream = new MemoryStream(bytes);

        var blocks = await _parser.ParseAsync($"file{ext}", stream);

        var block = blocks.Should().ContainSingle().Which.Should().BeOfType<ImageBlock>().Subject;
        block.MimeType.Should().Be(expectedMime);
        block.Data.Should().Equal(bytes);
    }
}

public class PdfParserCanParseTests
{
    private readonly PdfParser _parser = new();

    [Theory]
    [InlineData("report.pdf",  null,              true)]
    [InlineData("REPORT.PDF",  null,              true)]
    [InlineData("file",        "application/pdf", true)]
    [InlineData("report.docx", null,              false)]
    [InlineData("file",        "text/plain",      false)]
    public void CanParse_ReturnsExpected(string fileName, string? mimeType, bool expected)
    {
        _parser.CanParse(fileName, mimeType).Should().Be(expected);
    }
}

public class WordParserCanParseTests
{
    private readonly WordParser _parser = new();

    [Theory]
    [InlineData("doc.docx", null,                                                                                              true)]
    [InlineData("DOC.DOCX", null,                                                                                              true)]
    [InlineData("file",     "application/vnd.openxmlformats-officedocument.wordprocessingml.document",                        true)]
    [InlineData("doc.doc",  null,                                                                                              false)]
    [InlineData("file",     "application/msword",                                                                              false)]
    public void CanParse_ReturnsExpected(string fileName, string? mimeType, bool expected)
    {
        _parser.CanParse(fileName, mimeType).Should().Be(expected);
    }
}

public class ExcelParserCanParseTests
{
    private readonly ExcelParser _parser = new();

    [Theory]
    [InlineData("data.xlsx", null,                                                                                 true)]
    [InlineData("DATA.XLSX", null,                                                                                 true)]
    [InlineData("file",      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",                 true)]
    [InlineData("data.xls",  null,                                                                                 false)]
    [InlineData("data.csv",  null,                                                                                 false)]
    public void CanParse_ReturnsExpected(string fileName, string? mimeType, bool expected)
    {
        _parser.CanParse(fileName, mimeType).Should().Be(expected);
    }
}

public class PowerPointParserCanParseTests
{
    private readonly PowerPointParser _parser = new();

    [Theory]
    [InlineData("slides.pptx", null,                                                                                              true)]
    [InlineData("SLIDES.PPTX", null,                                                                                              true)]
    [InlineData("file",        "application/vnd.openxmlformats-officedocument.presentationml.presentation",                      true)]
    [InlineData("slides.ppt",  null,                                                                                              false)]
    [InlineData("file",        "application/vnd.ms-powerpoint",                                                                   false)]
    public void CanParse_ReturnsExpected(string fileName, string? mimeType, bool expected)
    {
        _parser.CanParse(fileName, mimeType).Should().Be(expected);
    }
}
