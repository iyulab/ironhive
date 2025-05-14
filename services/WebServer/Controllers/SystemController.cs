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
    private readonly JobObject _job;
    private readonly ProcessStore _processes;

    public SystemController(JobObject job, ProcessStore processes)
    {
        _job = job;
        _processes = processes;
    }

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

    [HttpGet("process")]
    public async Task<ActionResult> GetProcessAsync()
    {
        var root = Process.GetCurrentProcess();

        var startInfo = new ProcessStartInfo
        {
            FileName = "notepad.exe",
            UseShellExecute = true,
        };

        var server1 = Process.Start(startInfo)!;
        var server2 = Process.Start(startInfo)!;

        _processes.AddProcess("server1", server1);
        _processes.AddProcess("server2", server2);

        //_job.AddProcess(server1);
        //_job.AddProcess(server2);

        //ProcessLauncher.Start("notepad.exe");
        //ProcessLauncher.Start("notepad.exe");

        Console.WriteLine($"Process ID: {server1.Id}");
        Console.WriteLine($"Process ID: {server2.Id}");

        await Task.CompletedTask;
        return Ok();
    }

    [HttpGet("crash")]
    public void Crash()
    {
        Environment.FailFast("비정상 종료 테스트");
    }
}
