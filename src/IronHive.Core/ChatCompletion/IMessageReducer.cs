using IronHive.Abstractions.ChatCompletion.Messages;

namespace IronHive.Core.ChatCompletion;

public interface IMessageReducer
{
    void Reduce(MessageContext context);
}
