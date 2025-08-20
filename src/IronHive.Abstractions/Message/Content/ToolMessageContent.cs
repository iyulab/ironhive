using IronHive.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message.Content;

/// <summary>
/// 도구(툴) 실행 상태를 나타내는 열거형입니다.
/// </summary>
public enum ToolContentStatus
{
    Waiting, // 도구가 대기 중인 상태입니다. (초기 상태 or 실행 이전 상태)
    Paused, // 도구가 일시 중지된 상태입니다. 이 상태는 사용자 승인이 요구되는 상태입니다.
    Approved, // 도구가 승인된 상태입니다.
    Rejected, // 도구가 거부된 상태입니다.
    InProgress, // 도구가 실행 중인 상태입니다.
    Success,  // 도구 실행이 성공한 상태입니다. (결과가 정상적으로 반환된 상태입니다.)
    Failure, // 도구 실행이 실패한 상태입니다. (오류가 발생한 상태입니다.)
}

/// <summary>
/// 도구(툴) 실행을 위한 메시지 콘텐츠 블록입니다.
/// </summary>
public class ToolMessageContent : MessageContent
{
    private string? _output;

    /// <summary>
    /// 블록의 고유 ID입니다. 서비스 제공자가 ID를 제공할 경우 해당 ID를 사용합니다.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 도구를 식별하기 위한 유니크한 이름입니다.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// 도구 이름입니다.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 도구 실행에 필요한 매개변수입니다.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// 도구의 상태를 나타냅니다.
    /// </summary>
    [JsonInclude]
    public ToolContentStatus Status { get; private set; } = ToolContentStatus.Waiting;

    /// <summary>
    /// 현재 도구의 결과 데이터입니다.
    /// </summary>
    [JsonInclude]
    public string? Output 
    {
        get
        {
            return Status switch
            {
                ToolContentStatus.Success or ToolContentStatus.Failure => _output,
                ToolContentStatus.Rejected => "This tool has been rejected by the user.",
                _ => null,
            };
        }
        private set
        {
            _output = value;
        }
    }

    /// <summary>
    /// 도구가 실행 완료되었는지 여부입니다.
    /// </summary>
    public bool IsCompleted => Status is ToolContentStatus.Success or ToolContentStatus.Failure or ToolContentStatus.Rejected;

    /// <summary>
    /// 현재 도구의 실패 여부를 확인합니다.
    /// </summary>
    public bool IsError => Status is ToolContentStatus.Failure or ToolContentStatus.Rejected;

    /// <summary>
    /// 도구를 대기 상태로 변경합니다.
    /// </summary>
    public void ChangeToWaiting() => Status = ToolContentStatus.Waiting;

    /// <summary>
    /// 도구를 일시 정지 상태로 변경합니다.
    /// </summary>
    public void ChangeToPaused() => Status = ToolContentStatus.Paused;

    /// <summary>
    /// 도구를 실행 승인 상태로 변경합니다.
    /// </summary>
    public void ChangeToApproved() => Status = ToolContentStatus.Approved;

    /// <summary>
    /// 도구를 실행 상태로 변경합니다.
    /// </summary>
    public void ChangeToInProgress() => Status = ToolContentStatus.InProgress;

    /// <summary>
    /// 도구 실행결과를 반영하여 상태를 변경합니다.
    /// </summary>
    public void CompleteExecution(ToolOutput output)
    {

        if (output.IsSuccess)
        {
            Status = ToolContentStatus.Success;
            Output = output.Data;
        }
        else
        {
            Status = ToolContentStatus.Failure;
            Output = $"Execution failed: {output.Data}";
        }
    }

    /// <summary>
    /// 현재 도구가 승인을 요구하는지 확인합니다.
    /// </summary>
    public bool RequiresApproval(IEnumerable<ITool> tools)
    {
        var tool = tools.FirstOrDefault(d => d.Key == Key);
        if (tool == null)
            throw new InvalidOperationException($"Tool descriptor not found for tool: {Name}");
        
        return tool.RequiresApproval;
    }
}
