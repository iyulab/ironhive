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
    // Replacing a client swaps its tools as one step, so a concurrent update or removal of the same name
    // cannot interleave between removing the old tools and adding the new ones.
    private readonly Lock _gate = new();

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
    /// OpenAPI 클라이언트를 추가하거나 같은 이름의 기존 클라이언트를 교체하고, 그 클라이언트의 도구를
    /// 도구 컬렉션에 반영합니다.
    /// </summary>
    /// <remarks>
    /// 도구 목록을 먼저 만든 뒤에 등록한다. 목록을 만들지 못하면(예: 스펙에 <c>servers</c> 가 없다) 예외가
    /// 호출자에게 전달되고 아무것도 바뀌지 않는다 — 기존 클라이언트와 그 도구가 그대로 남고, 넘겨받은
    /// <paramref name="client"/> 는 등록되지 않는다(해제는 호출자의 몫). 교체에 성공하면 이전 클라이언트의 도구를
    /// 새 도구로 바꾼 뒤 이전 클라이언트를 해제한다.
    /// </remarks>
    /// <param name="client">OpenApi의 클라이언트 객체입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task AddOrUpdateAsync(OpenApiClient client, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var tools = (await client.ListToolsAsync(cancellationToken).ConfigureAwait(false)).ToList();

        OpenApiClient? previous;
        lock (_gate)
        {
            _clients.TryGetValue(client.ClientName, out previous);
            _clients[client.ClientName] = client;
            _tools.RemoveAll(t => t is OpenApiTool ot && ot.ClientName.Equals(client.ClientName, StringComparison.Ordinal));
            _tools.SetRange(tools);
        }

        if (previous is not null && !ReferenceEquals(previous, client))
            previous.Dispose();
    }

    /// <summary>
    /// 지정된 이름의 클라이언트를 제거합니다.
    /// 클라이언트에 연결된 도구들도 함께 제거됩니다.
    /// </summary>
    /// <param name="clientName">제거할 클라이언트 이름</param>
    public void Remove(string clientName)
    {
        OpenApiClient? client;
        lock (_gate)
        {
            if (!_clients.TryRemove(clientName, out client))
                return;
            // 클라이언트의 도구 제거
            _tools.RemoveAll(t => t is OpenApiTool ot && ot.ClientName.Equals(clientName, StringComparison.Ordinal));
        }
        // 리소스 해제
        client.Dispose();
    }
}