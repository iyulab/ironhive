﻿using IronHive.Abstractions.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebServer.Controllers;

namespace WebServer.Dev;

/// <summary>
/// 개발 디버그 && 추적용 미들웨어
/// </summary>
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
        var type = context.Request.ContentType;

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
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {body}");
        }
        else if (type != null && type.Contains("multipart/form-data"))
        {
            //var form = await context.Request.ReadFormAsync();
            //var fileNames = form.Files.Select(f => f.FileName);
            //_logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} {string.Join(", ", fileNames)}");
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path} [multipart/form-data]");
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
                _logger.LogError($"JSON 파싱 오류: {ex.Message}");
            }
            finally
            {
                context.Request.Body.Position = 0;
            }
            _logger.LogInformation($"[Request] {context.Request.Method} {context.Request.Path}");
        }

        // 다음 미들웨어 호출
        await _next(context);
    }
}
