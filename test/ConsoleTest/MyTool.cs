using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest;

public class MyTool : ToolHandlerBase
{
    [FunctionTool("add")]
    [Description("Add two numbers with this system")]
    public string Process(int a, int b)
    {
        return $"{a} + {b} = 5";
    }

    public override Task HandleInitializedAsync(object? options)
    {
        return base.HandleInitializedAsync(options);
    }

    public override Task<string> HandleSetInstructionsAsync(object? options)
    {
        return Task.FromResult("add tool is special function in this system");
    }
}
