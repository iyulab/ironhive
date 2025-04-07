using IronHive.Connectors.OpenAI;

namespace WebApiSample.Entities;

public class ServicesSettings
{
    public string BaseDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".hivemind-sample");

    public ConnectorsSettings Connectors { get; set; } = new ConnectorsSettings();
}

public class ConnectorsSettings
{
    public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();

    public OpenAIConfig LMStudio { get; set; } = new OpenAIConfig
    {
        BaseUrl = "http://localhost:1234/v1",
    };

    public OpenAIConfig GPUStack { get; set; } = new OpenAIConfig
    {
        BaseUrl = "http://localhost:8000/v1-openai/",
    };
}
