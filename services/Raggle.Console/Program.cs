using Raggle.Console;
using Raggle.Console.Systems;
using Raggle.Console.UI;
using System.CommandLine;

var rootCommand = new RootCommand("A simple console app that says hello");

var pathOption = new Option<string>(
    aliases: ["--path", "-p"],
    description: "directory path to use as the base",
    isDefault: true,
    parseArgument: result => 
    {
        if (result.Tokens.Count == 0)
            return Directory.GetCurrentDirectory();
        else if (result.Tokens.Count > 1)
            throw new ArgumentException("Just one directory path is allowed");
        else if (Directory.Exists(result.Tokens[0].Value) == false)
            throw new ArgumentException("Directory does not exist");
        else
            return result.Tokens[0].Value;
    });

var initOption = new Option<bool>(
    aliases: ["--init", "-i"],
    description: "initialize the base directory");

rootCommand.AddOption(pathOption);
rootCommand.AddOption(initOption);

rootCommand.SetHandler(async (path, init) =>
{
    var builder = new AppBuilder(path);
    if (init)
    {
        builder.DeleteConfig();
    }
    
    var settings = builder.GetSettings();

    var fs = new FileSystem();
    await fs.Initialize(settings.WorkingDirectory);
    fs.Watch(settings.WorkingDirectory);

    var chat = new ChatUI();
    await chat.StartAsync();

}, pathOption, initOption);

return rootCommand.Invoke(args);
