using Raggle.Server.API.Models;

namespace Raggle.Server.Web.Models;

public class History
{
    public IEnumerable<Message> messages { get; set; }
}

public class Message
{
    public IEnumerable<Reference> references { get; set; }
    public string Content { get; set; }
}

public class Reference
{
    public Guid SourceId { get; set; }
    public IEnumerable<FileMeta> Files { get; set; }
}
