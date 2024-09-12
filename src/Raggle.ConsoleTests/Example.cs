using Raggle.Abstractions.Tools;

namespace Raggle.ConsoleTests;

internal class Example
{
    private readonly DepenDency _dency;
    public string Name { get; set; } = "홍길동";
    public int Age { get; set; } = 56;
    public string Plag { get; init; }

    public Example(DepenDency dency, string plag)
    {
        _dency = dency;
        Plag = plag;
    }

    [FunctionTool("Print")]
    public void Print()
    {
        Console.WriteLine($"Name: {Name}, Age: {Age}, Plag: {Plag}");
    }

    [FunctionTool("PrintDependency")]
    public DepenDency PrintDependency(params string[] args)
    {
        _dency.Print();
        Console.WriteLine($"Arg: {args[0]}, Arg2: {args[1]}");
        return _dency;
    }

    [FunctionTool("묘")]
    public async Task<DepenDency> PrintDependencyAsync(string arg, TheClass the)
    {
        _dency.Print();
        await Task.Delay(5_000);
        Console.WriteLine($"Arg: {arg}, Arg2: {the.Name}");
        return _dency;
    }
}

public class DepenDency
{
    public string Name { get; set; } = "세종대왕";

    public void Print()
    {
        Console.WriteLine("Dependency Print");
        Console.WriteLine($"Name: {Name}");
    }
}

public class TheClass
{
    public string Name { get; set; } = "더 클래스";
}