using System.Text;

namespace Raggle.Server.WebApi.Development;

public class ControllerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ControllerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<ControllerMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        // 요청 본문을 읽기 위해 스트림을 재설정 가능하게 함
        context.Request.EnableBuffering();

        // 요청 본문 읽기
        string body = "";
        using (var reader = new StreamReader(context.Request.Body,
                                             encoding: Encoding.UTF8,
                                             detectEncodingFromByteOrderMarks: false,
                                             bufferSize: 1024,
                                             leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            // 스트림 위치를 처음으로 되돌림
            context.Request.Body.Position = 0;
        }

        // 요청 정보 로그에 기록
        _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {body}");

        // 다음 미들웨어 호출
        await _next(context);
    }
}
