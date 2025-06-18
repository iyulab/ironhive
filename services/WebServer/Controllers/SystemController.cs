using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebServer.Controllers;

[ApiController]
[Route("/system")]
public class SystemController : ControllerBase
{
    [HttpGet("healthz")]
    public async Task<ActionResult> GetHealthAsync()
    {
        await Task.CompletedTask;
        return Ok("ok");
    }

    [HttpGet("time")]
    public async Task<ActionResult> GetTimeAsync()
    {
        await Task.CompletedTask;
        return Ok(DateTime.UtcNow);
    }

    [HttpGet("version")]
    public async Task<ActionResult> GetVersionAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            return StatusCode(500, "Unable to determine the application.");
        }

        var version = assembly.GetName().Version;
        await Task.CompletedTask;
        return Ok(version);
    }

    [HttpGet("crash")]
    public void Crash()
    {
        Environment.FailFast("비정상 종료 테스트");
    }

    [HttpGet("proxy")]
    public void Proxy()
    {
        ProxyManager.AddProxy("http://localhost:7777");
    }
}

public static class ProxyManager
{
    private static Dictionary<string, WebApplication> _apps = new Dictionary<string, WebApplication>();

    public static string BaseUrl { get; set; } = "http://localhost:5075";

    public static void AddProxy(string url)
    {
        if (_apps.ContainsKey(url))
        {
            throw new InvalidOperationException($"Proxy for {url} already exists.");
        }
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions());
        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            var message = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(BaseUrl + context.Request.Path + context.Request.QueryString),
                Content = new StreamContent(context.Request.Body)
            };
            foreach (var header in context.Request.Headers)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            using (var response = await new HttpClient().SendAsync(message))
            {
                context.Response.StatusCode = (int)response.StatusCode;
                foreach (var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await context.Response.WriteAsync(content);
                }
            }
            await next.Invoke();
        });
        _ = app.RunAsync(url);
        _apps[url] = app;
    }

    public static void RemoveProxy(string url)
    {
        if (!_apps.ContainsKey(url))
        {
            throw new InvalidOperationException($"Proxy for {url} does not exist.");
        }
        _apps[url].StopAsync().GetAwaiter().GetResult();
        _apps.Remove(url);
    }
}