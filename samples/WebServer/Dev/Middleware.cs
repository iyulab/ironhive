using IronHive.Abstractions.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebServer.Controllers;

namespace WebServer.Dev;

/// <summary>
/// 개발 디버그 && 추적용 미들웨어
/// </summary>
public partial class Middleware
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
        var type = context.Request.ContentType;
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();

        // 요청 본문 읽기
        if (type == "application/json")
        {
            using var reader = new StreamReader(context.Request.Body,
                                                 encoding: Encoding.UTF8,
                                                 detectEncodingFromByteOrderMarks: false,
                                                 bufferSize: 1024,
                                                 leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            // 스트림 위치를 처음으로 되돌림
            context.Request.Body.Position = 0;
            LogRequestWithBody(_logger, method, path, body);
        }
        else if (type != null && type.Contains("multipart/form-data"))
        {
            //var form = await context.Request.ReadFormAsync();
            //var fileNames = form.Files.Select(f => f.FileName);
            //_logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {string.Join(", ", fileNames)}");
            LogRequestMultipart(_logger, method, path);
        }
        else
        {
            var requestType = typeof(MessageGenerationRequest);
            try
            {
                // JSON 요청 본문을 파싱
                using var reader = new StreamReader(context.Request.Body,
                                                 encoding: Encoding.UTF8,
                                                 detectEncodingFromByteOrderMarks: false,
                                                 bufferSize: 1024,
                                                 leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                var options = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
                var jsonOptions = options.Value.JsonSerializerOptions;
                var json = JsonSerializer.Deserialize(body, requestType, jsonOptions);
            }
            catch (JsonException ex)
            {
                LogJsonParseError(_logger, ex.Message);
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
            LogRequest(_logger, method, path);
        }

        // 다음 미들웨어 호출
        await _next(context);
    }

    #region LoggerMessage

    [LoggerMessage(Level = LogLevel.Information, Message = "[Request] {Method} {Path} {Body}")]
    private static partial void LogRequestWithBody(ILogger logger, string method, string path, string body);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Request] {Method} {Path} [multipart/form-data]")]
    private static partial void LogRequestMultipart(ILogger logger, string method, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Request] {Method} {Path}")]
    private static partial void LogRequest(ILogger logger, string method, string path);

    [LoggerMessage(Level = LogLevel.Error, Message = "JSON 파싱 오류: {ErrorMessage}")]
    private static partial void LogJsonParseError(ILogger logger, string errorMessage);

    #endregion
}
