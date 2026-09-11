using System.Globalization;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Services;

/// <summary>
/// 한 번의 <see cref="IMessageService"/> 호출(도구 루프 전체) 동안 모델에 보내는 도구 결과 텍스트의 합계를 제한하는 메시지 미들웨어입니다.
/// </summary>
/// <remarks>
/// <para>
/// 결과 하나마다 상한을 두는 것(<c>ToolOptions.OnAfterInvoke</c> + <see cref="Utilities.TextCompactor"/>)으로는
/// 여러 라운드에 걸쳐 쌓이는 합계를 막을 수 없습니다. 이 미들웨어는 매 턴 제너레이터를 부르기 직전에, 이번 호출에서
/// 실행된 도구 결과를 도착 순서대로 훑어 예산을 배분합니다 — 예산보다 긴 결과는 남은 만큼으로 자르고,
/// 남은 예산이 없으면 결과를 <see cref="ExhaustedNotice"/>로 바꿉니다. 예산이 소진된 턴부터는 요청의
/// <see cref="MessageGenerationRequest.ToolChoice"/>를 <see cref="ToolChoice.None"/>으로 두어 모델이 받은 결과로 답하게 합니다.
/// </para>
/// <para>
/// 텍스트 콘텐츠만 셉니다. 이미지 같은 비텍스트 콘텐츠는 그대로 보냅니다. 대체 문구는 예산에 포함되지 않습니다.
/// 도구가 모두 끝난 뒤 턴마다 한 번 실행되므로 <c>ToolOptions.MaxParallel</c>과 무관하게 결과가 결정적으로 배분됩니다.
/// 결과는 제자리에서 바뀌므로 최종 응답 메시지에도 줄어든 결과가 남습니다.
/// </para>
/// <para>
/// <c>HiveServiceBuilder.AddMessageMiddleware(new ToolResultBudgetMiddleware(maxTotalChars))</c>로 등록합니다.
/// </para>
/// </remarks>
public sealed class ToolResultBudgetMiddleware : IMessageMiddleware
{
    /// <summary>
    /// 예산이 남지 않은 결과 대신 보내는 기본 문구입니다.
    /// </summary>
    public const string DefaultExhaustedNotice =
        "[Tool result budget exhausted: this result was not included. Do not call more tools; answer with the results you already have.]";

    /// <summary>
    /// 예산을 지정해 미들웨어를 만듭니다.
    /// </summary>
    /// <param name="maxTotalChars">한 호출 동안 모델에 보내는 도구 결과 텍스트의 최대 문자 수입니다(1 이상).</param>
    public ToolResultBudgetMiddleware(int maxTotalChars)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTotalChars, 1);
        MaxTotalChars = maxTotalChars;
    }

    /// <summary>
    /// 한 호출 동안 모델에 보내는 도구 결과 텍스트의 최대 문자 수입니다.
    /// </summary>
    public int MaxTotalChars { get; }

    /// <summary>
    /// 예산이 남지 않은 결과 대신 보내는 문구입니다. 기본값은 <see cref="DefaultExhaustedNotice"/>입니다.
    /// </summary>
    public string ExhaustedNotice { get; init; } = DefaultExhaustedNotice;

    /// <inheritdoc />
    public Task<MessageResponse> GenerateAsync(
        MessageContext context,
        Func<MessageContext, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        Apply(context);
        return next(context);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingAsync(
        MessageContext context,
        Func<MessageContext, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        Apply(context);
        return next(context);
    }

    // 이미 예산 안에 맞춘 이전 턴의 결과는 같은 자리에서 같은 몫을 다시 받으므로 바뀌지 않는다(멱등).
    private void Apply(MessageContext context)
    {
        var remaining = MaxTotalChars;

        foreach (var tool in context.CurrentMessage?.Content.OfType<ToolMessageContent>() ?? [])
        {
            if (tool.Output is not { } output)
                continue;

            var length = TextLength(output);
            if (length <= remaining)
            {
                remaining -= length;
                continue;
            }

            output.Content = Fit(output.Content, remaining, length);
            remaining = 0;
        }

        if (remaining == 0)
            context.Request.ToolChoice = ToolChoice.None;
    }

    private IReadOnlyList<MessageContent> Fit(IReadOnlyList<MessageContent> content, int remaining, int length)
    {
        var text = string.Concat(content.OfType<TextMessageContent>().Select(t => t.Value));
        var marker = string.Create(
            CultureInfo.InvariantCulture,
            $"\n[... truncated by the tool result budget ({length:N0} chars total) ...]");

        string fitted;
        if (remaining <= marker.Length)
        {
            fitted = ExhaustedNotice;
        }
        else
        {
            var cut = remaining - marker.Length;
            if (char.IsHighSurrogate(text[cut - 1]))
                cut--;
            fitted = string.Concat(text.AsSpan(0, cut), marker);
        }

        return [new TextMessageContent { Value = fitted }, .. content.Where(c => c is not TextMessageContent)];
    }

    private static int TextLength(ToolOutput output)
        => output.Content.OfType<TextMessageContent>().Sum(t => t.Value?.Length ?? 0);
}
