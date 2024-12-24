namespace Iyulab.Auth.Shared;

public class User
{
    public Guid Id { get; set; }

    public Role Role { get; set; }

    public string Username { get; set; } = string.Empty;

    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
