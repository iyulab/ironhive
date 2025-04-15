using System.Text.Json.Serialization;

namespace IronHive.Core.Mcp;

/// <summary>
/// 인터페이스로, 다양한 종류의 MCP 서버를 나타내는 공통 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(McpStdioServer), "stdio")]
[JsonDerivedType(typeof(McpSseServer), "sse")]
public interface IMcpServer
{
    /// <summary>
    /// 서버의 이름을 가져오거나 설정합니다.
    /// </summary>
    string? Name { get; set; }
}


/// <summary>
/// MCP 서버의 기본 클래스입니다. IMcpServer 인터페이스를 구현합니다.
/// </summary>
public abstract class McpServerBase : IMcpServer
{
    /// <inheritdoc />
    public string? Name { get; set; }
}




