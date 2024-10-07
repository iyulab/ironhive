using System.CommandLine;

var rootCommand = new RootCommand("A Raggle CLI Tool");

var pathOption = new Option<string>(
    aliases: ["--config", "-c"],
    description: "configuration file path",
    isDefault: true,
    parseArgument: result => 
    {
        if (result.Tokens.Count == 0)
            return Path.Combine(Directory.GetCurrentDirectory(), "RaggleSettings.json");
        else if (result.Tokens.Count > 1)
            throw new ArgumentException("There are multiple paths");
        else if (File.Exists(result.Tokens[0].Value) == false)
            throw new ArgumentException("File does not exist");
        else
            return result.Tokens[0].Value;
    });

var initOption = new Option<bool>(
    aliases: ["--init", "-i"],
    description: "initialize");

rootCommand.AddOption(pathOption);
rootCommand.AddOption(initOption);

rootCommand.SetHandler(async (path, init) =>
{
    if (init)
    {
        Console.WriteLine($"Initializing {path}");
        await Task.Delay(1000);
        Console.WriteLine("Initialization complete");
    }
    else
    {
        Console.WriteLine($"Using path {path}");
    }
}, pathOption, initOption);

return rootCommand.Invoke(args);
