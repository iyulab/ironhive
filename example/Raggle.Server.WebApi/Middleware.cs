using System.Text;

namespace Raggle.Server.WebApi;

public class Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public Middleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<Middleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        // 요청 본문을 읽기 위해 스트림을 재설정 가능하게 함
        context.Request.EnableBuffering();
        var cType = context.Request.ContentType;

        // 요청 본문 읽기
        if (cType == "application/json")
        {
            using var reader = new StreamReader(context.Request.Body,
                                                 encoding: Encoding.UTF8,
                                                 detectEncodingFromByteOrderMarks: false,
                                                 bufferSize: 1024,
                                                 leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            // 스트림 위치를 처음으로 되돌림
            context.Request.Body.Position = 0;
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {body}");
        }
        else if (cType != null && cType.Contains("multipart/form-data"))
        {
            //var form = await context.Request.ReadFormAsync();
            //var fileNames = form.Files.Select(f => f.FileName);
            //_logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {string.Join(", ", fileNames)}");
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} [multipart/form-data]");
        }
        else
        {
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path}");
        }

        // 다음 미들웨어 호출
        await _next(context);
    }
}
