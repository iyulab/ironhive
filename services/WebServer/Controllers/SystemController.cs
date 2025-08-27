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
        try
        {
            var healthInfo = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                uptime = Environment.TickCount64,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown",
                checks = new
                {
                    memory = new
                    {
                        status = "ok",
                        workingSet = GC.GetTotalMemory(false)
                    },
                    runtime = new
                    {
                        status = "ok",
                        framework = RuntimeInformation.FrameworkDescription,
                        osDescription = RuntimeInformation.OSDescription
                    }
                }
            };
            
            await Task.CompletedTask;
            return Ok(healthInfo);
        }
        catch (Exception ex)
        {
            var errorInfo = new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            };
            
            return StatusCode(503, errorInfo);
        }
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
        var buildTime = GetBuildTime();
        
        var versionInfo = new
        {
            version = version?.ToString() ?? "Unknown",
            buildTime = buildTime,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            runtime = new
            {
                framework = RuntimeInformation.FrameworkDescription,
                os = RuntimeInformation.OSDescription,
                architecture = RuntimeInformation.OSArchitecture.ToString()
            },
            deployment = new
            {
                startTime = Process.GetCurrentProcess().StartTime,
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
                workingDirectory = Environment.CurrentDirectory
            }
        };
        
        await Task.CompletedTask;
        return Ok(versionInfo);
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetDeploymentStatusAsync()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var status = new
            {
                application = new
                {
                    name = "IronHive",
                    status = "running",
                    version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                },
                system = new
                {
                    startTime = process.StartTime,
                    uptime = DateTime.UtcNow - process.StartTime,
                    processId = process.Id,
                    threads = process.Threads.Count,
                    workingSet = process.WorkingSet64,
                    gcMemory = GC.GetTotalMemory(false)
                },
                deployment = new
                {
                    deploymentTime = GetBuildTime(),
                    workingDirectory = Environment.CurrentDirectory,
                    runtime = RuntimeInformation.FrameworkDescription,
                    platform = RuntimeInformation.OSDescription
                }
            };
            
            await Task.CompletedTask;
            return Ok(status);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static DateTime GetBuildTime()
    {
        const string buildVersionMetadataPrefix = "+build";
        var attribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(buildVersionMetadataPrefix);
            if (index > 0)
            {
                var buildTimeString = value.Substring(index + buildVersionMetadataPrefix.Length);
                if (DateTime.TryParse(buildTimeString, out var buildTime))
                {
                    return buildTime;
                }
            }
        }
        
        // Fallback to file creation time
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null && !string.IsNullOrEmpty(assembly.Location))
            {
                return new FileInfo(assembly.Location).CreationTime;
            }
        }
        catch
        {
            // Ignore errors
        }
        
        return DateTime.MinValue;
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