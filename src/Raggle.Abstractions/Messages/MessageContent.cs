using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Abstractions.Messages;

public interface IMessageContent
{
}

public abstract class UserMessageContent : IMessageContent
{
}

public abstract class AssistantMessageContent : IMessageContent
{
}

public class TextMessageContent : UserMessageContent, AssistantMessageContent
{
    public string Text { get; set; }
}

public class ImageMessageContent : UserMessageContent
{
    public string Data { get; set; }
}

public class ToolCallMessageContent : AssistantMessageContent
{
    public string Name { get; set; }
    public object? Arguments { get; set; }
}

public class ToolResultMessageContent : AssistantMessageContent
{
    public object? Result { get; set; }
}
