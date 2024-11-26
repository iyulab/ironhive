using ConsoleTest;
using Raggle.Core.Memory.Decoders;
using System.Text.Json;

var oo = new MyClass
{
    Name = "MyName",
    Status = MyEnum.MyType2,
};
oo.SetList(new List<string> { "X", "Y", "Z" });

var json = JsonSerializer.Serialize(oo);
var oo2 = JsonSerializer.Deserialize<MyClass>(json);

var options = new JsonSerializerOptions
{

};

return;
