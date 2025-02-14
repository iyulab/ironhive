namespace Raggle.Abstractions.Messages;

public interface IMessageCondenser
{
    Task<string> CondenseAsync(string message, int maxLength);
}
