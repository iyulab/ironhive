using System.Data;
using System.Text;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace IronHive.Plugins.MCP;

/// <summary>
/// MCP(ModelContext Protocol) 클라이언트를 기반으로 동작하는 도구 구현체입니다.
///
/// 이 클래스는 IronHive 환경에서 MCP 서버와 통신하기 위한 Tool 역할을 수행합니다.
/// MCP 클라이언트를 감싸서(IronHive.Abstractions.Tools.ITool 인터페이스 구현)
/// IronHive의 표준화된 툴 호출 방식에 맞게 래핑(wrapping)합니다.
/// </summary>
public class McpTool : ITool
{
    private readonly McpClientTool _tool;
    
    public McpTool(McpClientTool tool)
    {
        _tool = tool ?? throw new ArgumentNullException(nameof(tool));
    }

    /// <inheritdoc />
    public string UniqueName => $"mcp_{ServerName}_{Name}";

    /// <summary>
    /// 현재 MCP 툴이 속한 서버의 이름입니다.
    /// </summary>
    public required string ServerName { get; init; }

    /// <summary>
    /// MCP 툴의 이름입니다.
    /// </summary>
    public string Name => _tool.Name;

    /// <inheritdoc />
    public string? Description => _tool.Description;

    /// <inheritdoc />
    public object? Parameters => _tool.JsonSchema;

    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = true;

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await _tool.CallAsync(
            arguments: input,
            progress: null,
            cancellationToken: cancellationToken);

        // 텍스트/이미지/오디오는 그대로 매핑되고, 그 외(임베디드 리소스의 인식하지 못하는 MIME 타입,
        // 중첩 tool_use/tool_result 등)는 설명 텍스트로 대체됩니다.
        var content = new List<MessageContent>();
        foreach (var block in result.Content)
        {
            MessageContent mapped = block switch
            {
                TextContentBlock text => new TextMessageContent { Value = text.Text },
                ImageContentBlock image => ToImageFormat(image.MimeType) is { } format
                    ? new ImageMessageContent { Format = format, Base64 = Encoding.UTF8.GetString(image.Data.Span) }
                    : new TextMessageContent { Value = $"[unsupported image content omitted — unrecognized MIME type '{image.MimeType}']" },
                AudioContentBlock audio => ToAudioFormat(audio.MimeType) is { } format
                    ? new AudioMessageContent { Format = format, Base64 = Encoding.UTF8.GetString(audio.Data.Span) }
                    : new TextMessageContent { Value = $"[unsupported audio content omitted — unrecognized MIME type '{audio.MimeType}']" },
                EmbeddedResourceBlock { Resource: TextResourceContents text } => new TextMessageContent { Value = text.Text },
                EmbeddedResourceBlock { Resource: BlobResourceContents blob } when blob.MimeType is not null && ToImageFormat(blob.MimeType) is { } imageFormat =>
                    new ImageMessageContent { Format = imageFormat, Base64 = Encoding.UTF8.GetString(blob.Blob.Span) },
                EmbeddedResourceBlock { Resource: BlobResourceContents blob } when blob.MimeType is not null && ToAudioFormat(blob.MimeType) is { } audioFormat =>
                    new AudioMessageContent { Format = audioFormat, Base64 = Encoding.UTF8.GetString(blob.Blob.Span) },
                EmbeddedResourceBlock { Resource: BlobResourceContents blob } =>
                    new TextMessageContent { Value = $"[unsupported resource content omitted — uri '{blob.Uri}', mimeType '{blob.MimeType}']" },
                _ => new TextMessageContent { Value = $"[unsupported {block.Type} content omitted]" }
            };
            content.Add(mapped);
        }

        // MCP protocol: IsError absent means success
        return new ToolOutput(result.IsError is not true, content);
    }

    private static ImageFormat? ToImageFormat(string? mimeType) => mimeType switch
    {
        "image/png" => ImageFormat.Png,
        "image/jpeg" or "image/jpg" => ImageFormat.Jpeg,
        "image/gif" => ImageFormat.Gif,
        "image/webp" => ImageFormat.Webp,
        _ => null
    };

    private static AudioFormat? ToAudioFormat(string? mimeType) => mimeType switch
    {
        "audio/wav" or "audio/x-wav" or "audio/wave" => AudioFormat.Wav,
        "audio/mp3" or "audio/mpeg" => AudioFormat.Mp3,
        "audio/flac" => AudioFormat.Flac,
        "audio/aac" => AudioFormat.Aac,
        "audio/ogg" => AudioFormat.Ogg,
        "audio/aiff" or "audio/x-aiff" => AudioFormat.Aiff,
        _ => null
    };
}
