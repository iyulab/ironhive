namespace Iyulab.Auth.Shared;

public class Token
{
    public Guid Id { get; set; }

    public string? Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }
}
