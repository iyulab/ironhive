using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;

namespace IronHive.Core.Agent;

/// <summary>
/// 에이전트 호출을 로깅하는 미들웨어입니다.
/// 스트리밍과 비스트리밍 모두 지원합니다.
/// </summary>
public class LoggingMiddleware : IAgentMiddleware, IStreamingAgentMiddleware
{
    private readonly LoggingMiddlewareOptions _options;

    public LoggingMiddleware(LoggingMiddlewareOptions? options = null)
    {
        _options = options ?? new LoggingMiddlewareOptions();
    }

    /// <summary>
    /// 로그 출력 함수만 지정하여 생성합니다.
    /// </summary>
    public LoggingMiddleware(Action<string> logAction)
        : this(new LoggingMiddlewareOptions { LogAction = logAction })
    {
    }

    public async Task<MessageResponse> InvokeAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageList = messages.ToList();

        // 시작 로그
        LogStart(agent, messageList, streaming: false);

        try
        {
            var response = await next(messageList).ConfigureAwait(false);
            stopwatch.Stop();

            // 완료 로그
            LogComplete(agent, response, stopwatch.Elapsed);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // 실패 로그
            LogError(agent, ex, stopwatch.Elapsed);

            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<IEnumerable<Message>, IAsyncEnumerable<StreamingMessageResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageList = messages.ToList();

        // 시작 로그
        LogStart(agent, messageList, streaming: true);

        var hasError = false;
        Exception? error = null;

        await using var enumerator = next(messageList).GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            StreamingMessageResponse? current;
            try
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    break;
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                hasError = true;
                error = ex;
                stopwatch.Stop();
                LogError(agent, ex, stopwatch.Elapsed);
                throw;
            }

            yield return current;
        }

        stopwatch.Stop();

        if (!hasError)
        {
            LogStreamingComplete(agent, stopwatch.Elapsed);
        }
    }

    private void LogStart(IAgent agent, IReadOnlyList<Message> messages, bool streaming = false)
    {
        if (_options.LogAction == null)
            return;

        var messageCount = messages.Count;
        var lastMessage = messages.LastOrDefault();
        var preview = GetMessagePreview(lastMessage);
        var mode = streaming ? "streaming" : "invoke";

        var log = $"[{_options.LogPrefix}] Agent '{agent.Name}' starting ({mode}) " +
                  $"(messages: {messageCount}, model: {agent.Model})";

        if (_options.IncludeMessagePreview && !string.IsNullOrEmpty(preview))
        {
            log += $"\n  Last message: {preview}";
        }

        _options.LogAction(log);
    }

    private void LogStreamingComplete(IAgent agent, TimeSpan elapsed)
    {
        if (_options.LogAction == null)
            return;

        var log = $"[{_options.LogPrefix}] Agent '{agent.Name}' streaming completed " +
                  $"({elapsed.TotalMilliseconds:F0}ms)";

        _options.LogAction(log);
    }

    private void LogComplete(IAgent agent, MessageResponse response, TimeSpan elapsed)
    {
        if (_options.LogAction == null)
            return;

        var tokenInfo = response.TokenUsage != null
            ? $", tokens: {response.TokenUsage.InputTokens}+{response.TokenUsage.OutputTokens}"
            : "";

        var log = $"[{_options.LogPrefix}] Agent '{agent.Name}' completed " +
                  $"({elapsed.TotalMilliseconds:F0}ms{tokenInfo})";

        if (_options.IncludeResponsePreview)
        {
            var preview = GetResponsePreview(response);
            if (!string.IsNullOrEmpty(preview))
            {
                log += $"\n  Response: {preview}";
            }
        }

        _options.LogAction(log);
    }

    private void LogError(IAgent agent, Exception ex, TimeSpan elapsed)
    {
        if (_options.LogAction == null)
            return;

        var log = $"[{_options.LogPrefix}] Agent '{agent.Name}' failed " +
                  $"({elapsed.TotalMilliseconds:F0}ms): {ex.GetType().Name}: {ex.Message}";

        _options.LogAction(log);
    }

    private string? GetMessagePreview(Message? message)
    {
        if (message == null)
            return null;

        var content = message switch
        {
            IronHive.Abstractions.Messages.Roles.UserMessage um => um.Content,
            IronHive.Abstractions.Messages.Roles.AssistantMessage am => am.Content,
            _ => null
        };

        if (content == null)
            return null;

        var text = string.Join("", content.OfType<TextMessageContent>().Select(c => c.Value));
        return TruncateWithEllipsis(text, _options.MaxPreviewLength);
    }

    private string? GetResponsePreview(MessageResponse response)
    {
        var content = response.Message switch
        {
            IronHive.Abstractions.Messages.Roles.AssistantMessage am => am.Content,
            _ => null
        };

        if (content == null)
            return null;

        var text = string.Join("", content.OfType<TextMessageContent>().Select(c => c.Value));
        return TruncateWithEllipsis(text, _options.MaxPreviewLength);
    }

    private static string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // 줄바꿈을 공백으로 치환
        text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        if (text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }
}

/// <summary>
/// LoggingMiddleware 설정 옵션
/// </summary>
public class LoggingMiddlewareOptions
{
    /// <summary>
    /// 로그 출력 함수. null이면 로깅하지 않습니다.
    /// </summary>
    public Action<string>? LogAction { get; set; }

    /// <summary>
    /// 로그 접두사 (기본값: "Agent")
    /// </summary>
    public string LogPrefix { get; set; } = "Agent";

    /// <summary>
    /// 마지막 메시지 미리보기 포함 여부 (기본값: true)
    /// </summary>
    public bool IncludeMessagePreview { get; set; } = true;

    /// <summary>
    /// 응답 미리보기 포함 여부 (기본값: true)
    /// </summary>
    public bool IncludeResponsePreview { get; set; } = true;

    /// <summary>
    /// 미리보기 최대 길이 (기본값: 100)
    /// </summary>
    public int MaxPreviewLength { get; set; } = 100;
}
