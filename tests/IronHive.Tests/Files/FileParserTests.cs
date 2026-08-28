using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files.Parsers;

namespace IronHive.Tests.Files;

public class ImageParserTests
{
    private readonly ImageParser _parser = new();

    [Theory]
    [InlineData("photo.png",  true)]
    [InlineData("photo.jpg",  true)]
    [InlineData("photo.jpeg", true)]
    [InlineData("photo.gif",  true)]
    [InlineData("photo.webp", true)]
    [InlineData("photo.PNG",  true)]
    [InlineData("photo.svg",  false)]
    [InlineData("doc.pdf",    false)]
    public void CanParse_ReturnsExpected(string fileName, bool expected)
    {
        _parser.CanParse(fileName).Should().Be(expected);
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
    [InlineData("report.pdf",  true)]
    [InlineData("REPORT.PDF",  true)]
    [InlineData("report.docx", false)]
    public void CanParse_ReturnsExpected(string fileName, bool expected)
    {
        _parser.CanParse(fileName).Should().Be(expected);
    }
}

public class WordParserCanParseTests
{
    private readonly WordParser _parser = new();

    [Theory]
    [InlineData("doc.docx", true)]
    [InlineData("DOC.DOCX", true)]
    [InlineData("doc.doc",  false)]
    public void CanParse_ReturnsExpected(string fileName, bool expected)
    {
        _parser.CanParse(fileName).Should().Be(expected);
    }
}

public class ExcelParserCanParseTests
{
    private readonly ExcelParser _parser = new();

    [Theory]
    [InlineData("data.xlsx", true)]
    [InlineData("DATA.XLSX", true)]
    [InlineData("data.xls",  false)]
    [InlineData("data.csv",  false)]
    public void CanParse_ReturnsExpected(string fileName, bool expected)
    {
        _parser.CanParse(fileName).Should().Be(expected);
    }
}

public class PowerPointParserCanParseTests
{
    private readonly PowerPointParser _parser = new();

    [Theory]
    [InlineData("slides.pptx", true)]
    [InlineData("SLIDES.PPTX", true)]
    [InlineData("slides.ppt",  false)]
    public void CanParse_ReturnsExpected(string fileName, bool expected)
    {
        _parser.CanParse(fileName).Should().Be(expected);
    }
}
