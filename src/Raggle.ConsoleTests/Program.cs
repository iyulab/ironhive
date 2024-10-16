using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.StaticFiles;
using Raggle.Abstractions.Json;

async Task<string> GetKey()
{
    var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
    var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;
    return key;
}

var filePath = @"C:\data\Raggle\src\Raggle.ConsoleTests\Program.cs";
var fileInfo = new FileInfo(filePath);
var fileName = fileInfo.Name;

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".cs"] = "text/plain";
if (provider.TryGetContentType(fileName, out var contentType))
{
    Console.WriteLine($"Content type: {contentType}");
}
else
{
    Console.WriteLine("Content type not found");
}

var options = new JsonSerializerOptions
{
    Converters = { new JsonPolymorphicConverter<IMyInterface>("Time", new Dictionary<string, Type>()
    {
        { "Much", typeof(TwoClass) }
    })}
};

var one = new OneClass();
var two = new TwoClass();

var oneJson = JsonSerializer.Serialize<IMyInterface>(one, options);
var twoJson = JsonSerializer.Serialize<IMyInterface>(two, options);

Console.WriteLine(oneJson);
Console.WriteLine(twoJson);

var oneInstance = JsonSerializer.Deserialize<IMyInterface>(oneJson, options);
var twoInstance = JsonSerializer.Deserialize<IMyInterface>(twoJson, options);

Console.WriteLine(typeof(OneClass) == oneInstance.GetType());
Console.WriteLine(typeof(TwoClass) == twoInstance.GetType());

return;

[JsonDiscriminatorName("Go")]
public interface IMyInterface
{
    string MyProperty { get; set; }
}

public class OneClass : IMyInterface
{
    public string MyProperty { get; set; } = "This is One";

    public string MyName { get; set; } = "Hong";
}

[JsonDiscriminatorValue("TwoClass")]
public class TwoClass : IMyInterface
{
    public string MyProperty { get; set; } = "This is Two";

    public string MyValue { get; set; } = "Hello!";
} 
