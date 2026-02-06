using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.XAI;

/// <summary>
/// xAI (Grok) 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class XAIMessageGenerator : CompatibleResponseMessageGenerator
{
    private readonly XAIConfig _xaiConfig;

    public XAIMessageGenerator(XAIConfig config) : base(config)
    {
        _xaiConfig = config;
    }

    protected override T PostProcessRequest<T>(ResponsesRequest request)
    {
        // Store 설정
        if (_xaiConfig.Store.HasValue)
            request.Store = _xaiConfig.Store.Value;

        // 이전 응답 ID 연속
        if (!string.IsNullOrEmpty(_xaiConfig.PreviousResponseId))
            request.PreviousResponseId = _xaiConfig.PreviousResponseId;

        // 웹 검색 도구 주입
        if (_xaiConfig.EnableSearch)
        {
            var tools = request.Tools?.ToList() ?? [];
            var searchTool = new ResponsesWebSearchTool();

            if (_xaiConfig.SearchParameters != null)
            {
                searchTool.ContextSize = "medium";
            }

            tools.Add(searchTool);
            request.Tools = tools;
        }

        return (T)request;
    }
}
