using Microsoft.EntityFrameworkCore;

namespace Raggle.Server.WebApi.Models;

public class Collection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
