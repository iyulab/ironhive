using IronHive.Abstractions.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// LLM 도구(툴)를 정의하는 인터페이스입니다.
/// </summary>
[JsonConverter(typeof(PolymorphicJsonConverter<ITool>))]
[JsonPolymorphicName("type")]
public interface ITool
{
    /// <summary>
    /// LLM이 도구를 고유하게 식별하기 위한 이름입니다.
    /// 동일한 이름의 도구가 중복되지 않도록 보장합니다.
    /// </summary>
    string UniqueName { get; }

    /// <summary>
    /// 도구의 기능과 사용 방법을 설명합니다.
    /// LLM이 도구를 올바르게 활용할 수 있도록 안내하는 데 사용됩니다.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 도구 실행 시 전달되는 매개변수의 스키마(형식 정보)입니다.
    /// LLM이 어떤 입력을 전달해야 하는지 알 수 있도록 안내합니다.
    /// </summary>
    object? Parameters { get; }

    /// <summary>
    /// 도구 실행 전에 사용자의 승인이 필요한지 여부를 나타냅니다.
    /// true일 경우, LLM이 도구를 호출하기 전 승인 절차를 거쳐야 합니다.
    /// </summary>
    bool RequiresApproval { get; set; }

    /// <summary>
    /// 도구를 실행하는 비동기 메서드입니다.
    /// 입력 데이터를 받아 처리한 뒤, 결과를 <see cref="ToolOutput"/> 으로 반환합니다.
    /// </summary>
    Task<IToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default);
}