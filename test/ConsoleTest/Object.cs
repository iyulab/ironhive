using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ConsoleTest;

public enum MyEnum
{
    MyType1,
    MyType2,
    MyType3
}

public class MyClass
{
    public string Name { get; set; } = string.Empty;
    public MyEnum Status { get; set; } = MyEnum.MyType1;

    [JsonInclude]
    public List<string> List { get; private set; } = new List<string> { "A", "B", "C" };

    public void SetList(IEnumerable<string> list)
    {
        List = list.ToList();
    }
}
