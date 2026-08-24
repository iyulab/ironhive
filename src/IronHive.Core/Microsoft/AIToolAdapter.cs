using System.Text.Json;
using IronHive.Abstractions.Tools;
using Microsoft.Extensions.AI;

namespace IronHive.Core.Microsoft;

/// <summary>
/// M.E.AI의 AITool을 IronHive의 ITool로 변환하는 어댑터입니다.
/// ChatClientAdapter가 ChatOptions.Tools를 MessageGenerationRequest.Tools로 전달할 때
/// 내부적으로 쓰이며(그 경로에서는 실행을 FunctionInvokingChatClient가 가로채므로
/// 이 InvokeAsync는 호출되지 않습니다), 그와 별개로 MCP 클라이언트가 노출하는
/// McpClientTool 등 임의의 AIFunction을 IronHive 파이프라인(IHiveService.Messages,
/// Ironbees ProcessOptions 등)에서 직접 실행하려는 소비자를 위해 public으로 노출됩니다.
/// 감싼 AITool이 실행 가능한 AIFunction이 아니면(순수 선언용 도구) 실행할 대상이 없어
/// 실패로 반환합니다.
/// </summary>
public sealed class AIToolAdapter : ITool
{
    private readonly AITool _aiTool;

    public AIToolAdapter(AITool aiTool)
    {
        _aiTool = aiTool ?? throw new ArgumentNullException(nameof(aiTool));
    }

    public string UniqueName => _aiTool.Name;

    public string? Description => _aiTool.Description;

    public object? Parameters => _aiTool is AIFunctionDeclaration decl ? decl.JsonSchema : null;

    public bool RequiresApproval => false;

    public async Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        if (_aiTool is not AIFunction function)
        {
            return ToolOutput.Failure(
                $"'{_aiTool.Name}' has no executable AIFunction body — AIToolAdapter cannot invoke a declaration-only AITool.");
        }

        try
        {
            var arguments = new AIFunctionArguments(input.ToDictionary(kv => kv.Key, kv => kv.Value));
            var result = await function.InvokeAsync(arguments, cancellationToken).ConfigureAwait(false);

            return ToolOutput.Success(result switch
            {
                null => null,
                string s => s,
                _ => JsonSerializer.Serialize(result, function.JsonSerializerOptions)
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ToolOutput.Failure(ex.Message);
        }
    }
}
