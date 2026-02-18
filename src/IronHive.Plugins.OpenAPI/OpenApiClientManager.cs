using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using IronHive.Abstractions.Tools;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// OpenAPI 클라이언트를 관리하는 클래스입니다.
/// 클라이언트의 추가, 갱신, 조회 및 제거 기능을 제공하며,
/// 관련 도구(IToolCollection)와의 연동도 처리합니다.
/// </summary>
public sealed class OpenApiClientManager
{
    private readonly ConcurrentDictionary<string, OpenApiClient> _clients = new();
    private readonly IToolCollection _tools;

    public OpenApiClientManager(IToolCollection tools)
    {
        _tools = tools;
    }

    /// <summary>
    /// 현재 등록된 모든 OpenAPI 클라이언트 목록을 반환합니다.
    /// </summary>
    public IReadOnlyCollection<OpenApiClient> Clients => _clients.Values.ToArray();

    /// <summary>
    /// 지정된 이름의 클라이언트를 조회합니다.
    /// </summary>
    /// <param name="clientName">조회할 클라이언트 이름</param>
    /// <param name="client">조회된 클라이언트 인스턴스 (없으면 null)</param>
    /// <returns>클라이언트가 존재하면 true, 그렇지 않으면 false</returns>
    public bool TryGetClient(string clientName, [MaybeNullWhen(false)] out OpenApiClient client)
    {
        if (_clients.TryGetValue(clientName, out var c))
        {
            client = c;
            return true;
        }

        client = null;
        return false;
    }

    /// <summary>
    /// OpenAPI 클라이언트를 추가하거나 기존 클라이언트를 갱신합니다.
    /// 클라이언트에 연결된 도구들도 함께 갱신됩니다.
    /// </summary>
    /// <param name="client">OpenApi의 클라이언트 객체입니다.</param>
    public void AddOrUpdate(OpenApiClient client, CancellationToken cancellationToken = default)
    {
        _clients.AddOrUpdate(client.ClientName,
            (_) =>
            {
                // 새 클라이언트의 도구 등록
                client.ListToolsAsync(cancellationToken).ContinueWith(task =>
                {
                    _tools.SetRange(task.Result);
                });
                return client;
            },
            (_, oc) =>
            {
                // 새 도구 등록
                client.ListToolsAsync(cancellationToken).ContinueWith(task =>
                {
                    _tools.RemoveAll(t => t is OpenApiTool ot && ot.ClientName.Equals(client.ClientName, StringComparison.Ordinal));
                    _tools.SetRange(task.Result);
                });

                // 기존 클라이언트 리소스 해제
                oc.Dispose();
                return client;
            });
    }

    /// <summary>
    /// 지정된 이름의 클라이언트를 제거합니다.
    /// 클라이언트에 연결된 도구들도 함께 제거됩니다.
    /// </summary>
    /// <param name="clientName">제거할 클라이언트 이름</param>
    public void Remove(string clientName)
    {
        if (_clients.TryRemove(clientName, out var client))
        {
            // 클라이언트의 도구 제거
            _tools.RemoveAll(t => t is OpenApiTool ot && ot.ClientName.Equals(clientName, StringComparison.Ordinal));
            // 리소스 해제
            client.Dispose();
        }
    }
}