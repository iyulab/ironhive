namespace Raggle.Server.Web.Models;

public class User : BaseEntity
{
    public string? DeviceInfo { get; set; }
    public string? Locale { get; set; }
    public string? IPAddress { get; set; }
}
