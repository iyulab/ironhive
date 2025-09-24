using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// 다양한 방식으로 OpenAPI 문서를 불러와 <see cref="OpenApiClient"/> 인스턴스를 생성하는 팩토리 클래스입니다.
/// </summary>
public sealed class OpenApiClientFactory
{
    /// <summary>
    /// 이미 파싱된 <see cref="OpenApiDocument"/>를 사용하여 <see cref="OpenApiClient"/>를 생성합니다.
    /// </summary>
    /// <param name="clientName">생성할 클라이언트의 이름입니다.</param>
    /// <param name="document">파싱된 OpenAPI 문서입니다.</param>
    /// <param name="options">클라이언트 설정 옵션입니다.</param>
    /// <returns>설정된 <see cref="OpenApiClient"/> 인스턴스를 반환합니다.</returns>
    public static OpenApiClient Create(
        string clientName,
        OpenApiDocument document,
        OpenApiClientOptions? options = null)
    {
        return new OpenApiClient(document, options)
        {
            ClientName = clientName,
        };
    }

    /// <summary>
    /// 문자열로 된 OpenAPI 문서를 파싱하여 <see cref="OpenApiClient"/>를 생성합니다.
    /// </summary>
    /// <param name="clientName">생성할 클라이언트의 이름입니다.</param>
    /// <param name="content">OpenAPI 문서의 원본 문자열(JSON 또는 YAML 형식).</param>
    /// <param name="options">클라이언트 설정 옵션입니다.</param>
    public static OpenApiClient CreateFromString(
        string clientName,
        string content,
        OpenApiClientOptions? options = null)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        var (doc, dialog) = OpenApiDocument.Parse(content, settings: settings);

        if (dialog is not null && dialog.Errors.Count > 0)
            throw new Exception("OpenAPI 문서 파싱 실패: " + string.Join("; ", dialog.Errors.Select(e => e.Message)));
        if (doc is null)
            throw new Exception("OpenAPI 문서를 불러오지 못했습니다.");

        return Create(clientName, doc, options);
    }

    /// <summary>
    /// URL에서 OpenAPI 문서를 비동기적으로 불러와 <see cref="OpenApiClient"/>를 생성합니다.
    /// </summary>
    /// <param name="clientName">생성할 클라이언트의 이름입니다.</param>
    /// <param name="url">OpenAPI 문서가 위치한 URL입니다.</param>
    /// <param name="options">클라이언트 설정 옵션입니다.</param>
    /// <param name="cancellationToken">작업 취소를 위한 토큰입니다.</param>
    public static async Task<OpenApiClient> CreateFromUrlAsync(
        string clientName,
        string url,
        OpenApiClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var (doc, dialog) = await OpenApiDocument.LoadAsync(url: url, token: cancellationToken);

        if (dialog is not null && dialog.Errors.Count > 0)
            throw new Exception("OpenAPI 문서 파싱 실패: " + string.Join("; ", dialog.Errors.Select(e => e.Message)));
        if (doc is null)
            throw new Exception("OpenAPI 문서를 불러오지 못했습니다.");

        return Create(clientName, doc, options);
    }
}