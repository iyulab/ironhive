using IronHive.Abstractions.Messages;

namespace IronHive.Core.Messages;

/// <summary>
/// MessageGenerationRequest의 Tool 제한 검증 확장 메서드입니다.
/// </summary>
public static class ToolLimitValidationExtensions
{
    /// <summary>
    /// 요청의 Tool 수 제한을 검증합니다.
    /// </summary>
    /// <param name="request">검증할 요청</param>
    /// <returns>검증 결과</returns>
    public static ToolLimitValidationResult ValidateToolLimit(this MessageGenerationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var toolCount = request.Tools?.Count ?? 0;
        var maxTools = request.MaxTools;

        // 제한이 비활성화된 경우 (0 이하)
        if (maxTools <= 0)
        {
            return new ToolLimitValidationResult
            {
                IsValid = true,
                ActualCount = toolCount,
                MaxAllowed = int.MaxValue,
                Message = null
            };
        }

        var isValid = toolCount <= maxTools;
        string? message = isValid
            ? null
            : $"Tool limit exceeded: {toolCount} tools provided but maximum is {maxTools}.";

        return new ToolLimitValidationResult
        {
            IsValid = isValid,
            ActualCount = toolCount,
            MaxAllowed = maxTools,
            Message = message
        };
    }

    /// <summary>
    /// Tool 제한 정책에 따라 검증을 수행하고 적절한 조치를 취합니다.
    /// </summary>
    /// <param name="request">검증할 요청</param>
    /// <param name="onWarning">경고 시 호출될 콜백 (메시지 전달)</param>
    /// <returns>검증이 통과되었거나 Truncate 후 유효한 경우 true</returns>
    /// <exception cref="ToolLimitExceededException">Throw 정책이고 제한 초과 시</exception>
    public static bool ApplyToolLimitPolicy(
        this MessageGenerationRequest request,
        Action<string>? onWarning = null)
    {
        var result = request.ValidateToolLimit();

        if (result.IsValid)
        {
            return true;
        }

        return request.ToolLimitBehavior switch
        {
            ToolLimitBehavior.Ignore => true,

            ToolLimitBehavior.Warn =>
                WarnAndContinue(result.Message!, onWarning),

            ToolLimitBehavior.Throw =>
                throw new ToolLimitExceededException(result.ActualCount, result.MaxAllowed),

            ToolLimitBehavior.Truncate =>
                TruncateTools(request, result.MaxAllowed, onWarning),

            _ => true
        };
    }

    private static bool WarnAndContinue(string message, Action<string>? onWarning)
    {
        onWarning?.Invoke(message);
        return true;
    }

    private static bool TruncateTools(
        MessageGenerationRequest request,
        int maxTools,
        Action<string>? onWarning)
    {
        if (request.Tools == null)
        {
            return true;
        }

        var originalCount = request.Tools.Count;
        var toolsToKeep = request.Tools.Take(maxTools).ToList();

        // 기존 컬렉션을 수정하지 않고 새 컬렉션으로 대체하기 위해
        // IToolCollection 인터페이스의 구현에 따라 처리 필요
        // 현재는 경고만 남기고 원본 유지
        onWarning?.Invoke(
            $"Tool limit would truncate from {originalCount} to {maxTools} tools. " +
            $"Consider reducing tools or increasing MaxTools.");

        return true;
    }
}
