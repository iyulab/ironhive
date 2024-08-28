namespace Raggle.Server.Web.Models;

public class UserChatHistory
{

}

public class StreamMessage
{
    public IEnumerable<SearchReference> References { get; set; }
    public string Content { get; set; }
}

public class SearchReference
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double Relevance { get; set; }
    public string Content { get; set; }
}