﻿namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구(툴) 실행의 결과를 나타내는 클래스입니다.
/// 성공 여부와 결과 데이터(또는 오류 메시지)를 포함합니다.
/// </summary>
public class ToolOutput
{
    /// <summary>
    /// 도구 실행이 성공했는지를 나타냅니다.
    /// </summary>
    public bool IsSuccess { get; set; } = false;

    /// <summary>
    /// 도구 실행 결과 데이터입니다.
    /// 성공한 경우, 결과 데이터를 문자열(JSON 등)로 반환하며,
    /// 실패한 경우, 오류 메시지를 포함합니다.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// 기본 생성자입니다.
    /// </summary>
    public ToolOutput() { }

    /// <summary>
    /// 결과 상태와 데이터를 지정하여 객체를 생성합니다.
    /// </summary>
    /// <param name="isSuccess">성공 여부</param>
    /// <param name="data">결과 데이터 또는 오류 메시지</param>
    public ToolOutput(bool isSuccess, string? data)
    {
        IsSuccess = isSuccess;
        Data = data;
    }

    /// <summary>
    /// 도구 실행이 성공했을 때의 결과를 생성합니다.
    /// </summary>
    /// <param name="result">실행 결과 데이터</param>
    /// <returns>성공한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Success(string? result)
        => new(true, result);

    /// <summary>
    /// 도구 실행이 실패했을 때의 결과를 생성합니다.
    /// </summary>
    /// <param name="error">오류 메시지</param>
    /// <returns>실패한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Failure(string? error)
        => new(false, error);

    /// <summary>
    /// 지정한 이름의 도구를 찾을 수 없을 때의 오류 결과를 생성합니다.
    /// </summary>
    /// <param name="toolName">찾지 못한 도구 이름</param>
    /// <returns>실패한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput NotFound(string toolName)
        => Failure($"도구 [{toolName}]를 찾을 수 없습니다. 이름에 오타가 없는지 확인하거나 사용 가능한 도구 목록을 참조하세요.");

    /// <summary>
    /// 사용자가 도구 실행을 거부했을 때의 오류 결과를 생성합니다.
    /// </summary>
    /// <returns>실패한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput Rejected()
        => Failure("사용자가 이 도구 호출 요청을 거부했습니다. 사용자에게 사유를 확인하세요.");

    /// <summary>
    /// 도구 실행 결과가 너무 방대한 경우의 오류 결과를 생성합니다.
    /// </summary>
    /// <returns>실패한 <see cref="ToolOutput"/> 객체</returns>
    public static ToolOutput TooMuch()
        => Failure("결과 데이터가 너무 많습니다. 파라미터를 변경하거나 필터를 지정하여 더 적은 결과를 받아보세요.");
}
