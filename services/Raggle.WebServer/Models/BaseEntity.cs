namespace Raggle.Server.Web.Models;

public abstract class BaseEntity
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public abstract class UserEntity : BaseEntity
{
    public Guid UserID { get; set; }

    // Navigation Properties
    public User? User { get; set; }
}
