using Raggle.Abstractions.ChatCompletion.Messages;

namespace Raggle.Core.ChatCompletion;

public interface IMessageReducer
{
    void Reduce(MessageContext context);
}
