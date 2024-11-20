namespace Raggle.Abstractions.Tools;

public class ToolKit
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string System { get; set; }

    public FunctionTool[] Tools { get; set; }

    public ToolKit(string name, string description, FunctionTool[] tools)
    {
        Name = name;
        Description = description;
        Tools = tools;
    }
}
