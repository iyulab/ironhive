using System.Text.Json.Serialization;

namespace IronHive.Plugins.MCP.Configurations;

/// <summary>
/// Model Context Protocol(MCP) 서버를 위한 공통 인터페이스입니다.
/// MCP는 AI 모델과 외부 도구, 시스템, 데이터 소스 간의 통신을 표준화하는 프로토콜입니다.
/// 여러 유형의 MCP 서버 구현을 위한 다형성을 지원합니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(McpStdioClientConfig), "stdio")]
[JsonDerivedType(typeof(McpSseClientConfig), "sse")]
public interface IMcpClientConfig
{
    /// <summary>
    /// MCP 클라이언트가 생성될 때 자동으로 서버에 연결할지 여부를 나타냅니다.
    /// </summary>
    bool AutoConnectOnCreated { get; set; }
}