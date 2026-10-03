using System.IO.Compression;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Providers;

/// <summary>
/// Against a vision model behind a real OpenAI-compatible server (llama-server with a multimodal projector): a tool
/// returns a solid-colour image, and the model is asked its colour. With
/// <see cref="OpenAICompatibleConfig.CarryImageToolResultsAsUserMessage"/> on it can answer; off (the control) the image
/// never reaches it. Opt-in: set <c>IRONHIVE_VLM_ENDPOINT</c> (base URL, e.g. <c>http://127.0.0.1:8080</c>).
/// </summary>
[Trait("Category", "Integration")]
public sealed class ChatCompletionImageToolResultLiveTests
{
    private static readonly string? Endpoint = Environment.GetEnvironmentVariable("IRONHIVE_VLM_ENDPOINT");

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VisionModel_SeesTheToolsImage_OnlyWhenCarried(bool carry)
    {
        Assert.SkipWhen(string.IsNullOrWhiteSpace(Endpoint), "IRONHIVE_VLM_ENDPOINT is not set");

        using var generator = new OpenAICompatibleMessageGenerator(new OpenAICompatibleConfig
        {
            BaseUrl = Endpoint,
            CarryImageToolResultsAsUserMessage = carry,
        });
        var swatch = new ToolMessageContent
        {
            Id = "call_1",
            Name = "render_swatch",
            Input = "{}",
            Output = ToolOutput.Success(
            [
                new TextMessageContent { Value = "Rendered the swatch." },
                new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String(SolidPng(64, 0, 160, 0)) },
            ]),
            IsApproved = true,
        };

        var response = await generator.GenerateMessageAsync(new MessageGenerationRequest
        {
            Model = "local",
            MaxTokens = 512,
            Temperature = 0,
            Messages =
            [
                Message.User("Render the colour swatch, then tell me its colour in one word. If you cannot see the image, answer exactly: unknown."),
                Message.Assistant(swatch),
            ],
        }, TestContext.Current.CancellationToken);

        var answer = string.Concat(response.Message!.Content.OfType<TextMessageContent>().Select(t => t.Value)).ToLowerInvariant();
        TestContext.Current.SendDiagnosticMessage($"carry={carry}: {answer}");
        if (carry)
            answer.Should().Contain("green");
        else
            answer.Should().NotContain("green", "without carriage the image never reaches the model");
    }

    /// <summary>A solid-colour RGB PNG (8-bit, no filter).</summary>
    private static byte[] SolidPng(int size, byte r, byte g, byte b)
    {
        var raw = new byte[size * ((size * 3) + 1)];
        for (var y = 0; y < size; y++)
        {
            var row = y * ((size * 3) + 1);
            for (var x = 0; x < size; x++)
            {
                raw[row + 1 + (x * 3)] = r;
                raw[row + 2 + (x * 3)] = g;
                raw[row + 3 + (x * 3)] = b;
            }
        }

        using var idat = new MemoryStream();
        using (var z = new ZLibStream(idat, CompressionLevel.Optimal, leaveOpen: true))
            z.Write(raw);

        using var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, (uint)size);
        WriteBigEndian(ihdr, 4, (uint)size);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 2; // colour type RGB
        WriteChunk(png, "IHDR", ihdr);
        WriteChunk(png, "IDAT", idat.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        var header = new byte[8];
        WriteBigEndian(header, 0, (uint)data.Length);
        System.Text.Encoding.ASCII.GetBytes(type, 0, 4, header, 4);
        stream.Write(header);
        stream.Write(data);
        var crc = new byte[4];
        WriteBigEndian(crc, 0, Crc32([.. header.AsSpan(4, 4), .. data]));
        stream.Write(crc);
    }

    private static void WriteBigEndian(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc ^= b;
            for (var k = 0; k < 8; k++)
                crc = (crc & 1) != 0 ? 0xEDB88320u ^ (crc >> 1) : crc >> 1;
        }

        return ~crc;
    }
}
