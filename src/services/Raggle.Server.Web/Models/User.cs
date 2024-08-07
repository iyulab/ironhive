using Microsoft.SemanticKernel.ChatCompletion;

namespace Raggle.Server.API.Models;

public class User
{
    public required Guid ID { get; set; }
    public required ChatHistory ChatHistory { get; set; }
}