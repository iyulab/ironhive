using Raggle.Abstractions.Tools;
using System.ComponentModel;

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

public class MathTools
{
    [FunctionTool(Name = "Add", Description = "Adds two numbers together")]
    public int AddAsync(
        [Description("The first number to add")] int a,
        [Description("The second number to add")] int b)
    {
        return a + b;
    }

    [FunctionTool(Name = "Subtract", Description = "Subtracts the second number from the first number")]
    public int SubtractAsync(
        [Description("The number to subtract from")] int a,
        [Description("The number to subtract")] int b)
    {
        return a - b;
    }

    [FunctionTool(Name = "Multiply", Description = "Multiplies two numbers together")]
    public int MultiplyAsync(
        [Description("The first number to multiply")] int a,
        [Description("The second number to multiply")] int b)
    {
        return a * b;
    }

    [FunctionTool(Name = "Divide", Description = "Divides the first number by the second number")]
    public double DivideAsync(
        [Description("The dividend")] double a,
        [Description("The divisor")] double b)
    {
        if (b == 0)
            throw new DivideByZeroException("Error: Division by zero is not allowed.");

        return a / b;
    }

    [FunctionTool(Name = "Power", Description = "Raises a number to the power of another number")]
    public double PowerAsync(
        [Description("The base number")] double baseNumber,
        [Description("The exponent")] double exponent)
    {
        double result = Math.Pow(baseNumber, exponent);
        return result;
    }

    [FunctionTool(Name = "SquareRoot", Description = "Calculates the square root of a number")]
    public double SquareRootAsync(
        [Description("The number to find the square root of")] double number)
    {
        if (number < 0)
            throw new InvalidOperationException("Error: Square root is not defined for negative numbers.");

        double result = Math.Sqrt(number);
        return result;
    }

    [FunctionTool(Name = "Absolute", Description = "Returns the absolute value of a number")]
    public double AbsoluteAsync(
        [Description("The number to find the absolute value of")] double number)
    {
        double result = Math.Abs(number);
        return result;
    }
}

public enum TheEnum
{
    ww,
    tt,
    Oll_aa,
    GETET
}

public interface TheInterface
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public abstract class TheAbstractClass
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public class TheClass
{
    [Description("this is my name")]
    public required string Name { get; set; }
}

public struct TheStruct
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public record TheRecord([Description("this is my name")] string name);
