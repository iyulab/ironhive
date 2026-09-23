namespace IronHive.Core.Agent;

/// <summary>
/// 설정 파일(YAML/TOML/JSON)에서 <see cref="IronHive.Abstractions.Agent.AgentConfig"/>로 읽히는 키 목록입니다.
/// 역직렬화기는 모르는 키를 조용히 버리므로(오타·지원하지 않는 키가 무시된 채 에이전트가 만들어진다),
/// 로더는 역직렬화 전에 이 목록으로 키를 검사해 모르는 키를 거부합니다.
/// </summary>
internal static class AgentConfigKeys
{
    /// <summary>에이전트 테이블의 키. TOML 은 <c>defaultProvider</c>/<c>defaultModel</c> 별칭도 받습니다.</summary>
    private static readonly string[] AgentKeys =
        ["name", "description", "provider", "model", "instructions", "parameters"];

    private static readonly string[] TomlAgentAliases = ["defaultProvider", "defaultModel"];

    private static readonly string[] ParameterKeys =
        ["maxTokens", "temperature", "topP", "topK", "stopSequences"];

    /// <summary>
    /// 에이전트 테이블(루트 또는 <c>agent:</c> 하위)의 키와 <c>parameters</c> 하위 키를 검사합니다.
    /// </summary>
    /// <param name="agentKeys">에이전트 테이블의 키.</param>
    /// <param name="parameterKeys"><c>parameters</c> 테이블의 키(없으면 null).</param>
    /// <param name="format">오류 메시지용 형식 이름.</param>
    /// <param name="allowTomlAliases">TOML 별칭 허용 여부.</param>
    /// <param name="comparer">키 비교자(JSON 은 대소문자 무시).</param>
    public static void Check(
        IEnumerable<string> agentKeys,
        IEnumerable<string>? parameterKeys,
        string format,
        bool allowTomlAliases = false,
        StringComparer? comparer = null)
    {
        comparer ??= StringComparer.Ordinal;
        var allowed = allowTomlAliases ? AgentKeys.Concat(TomlAgentAliases).ToArray() : AgentKeys;

        foreach (var key in agentKeys)
        {
            if (allowed.Contains(key, comparer))
                continue;

            if (comparer.Equals(key, "tools") || comparer.Equals(key, "toolOptions"))
            {
                throw new NotSupportedException(
                    $"{format} agent config key '{key}' is not supported: an agent built from a config file " +
                    "has no name-to-ITool registry, so a declared tool would never execute. Set IAgent.Tools " +
                    "on the constructed agent instead (e.g. myToolCollection.FilterBy(names)), or use a " +
                    "framework that resolves tool names (e.g. Ironbees's AgentConfig.Tools + IronhiveOptions.Tools).");
            }

            throw new ArgumentException(
                $"Unknown {format} agent config key '{key}'. Allowed keys: {string.Join(", ", allowed)}.");
        }

        if (parameterKeys is null)
            return;

        foreach (var key in parameterKeys)
        {
            if (!ParameterKeys.Contains(key, comparer))
            {
                throw new ArgumentException(
                    $"Unknown {format} agent config key 'parameters.{key}'. Allowed keys: {string.Join(", ", ParameterKeys)}.");
            }
        }
    }
}
