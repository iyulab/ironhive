using IronHive.Abstractions.ChatCompletion.Tools;

namespace IronHive.Abstractions.ChatCompletion;

public interface IChatCompletionServiceBuilder
{
    IChatCompletionServiceBuilder AddConnector(string key, IChatCompletionConnector connector);

    IChatCompletionServiceBuilder AddTool(string key, IToolService service);

    IChatCompletionService Build();
}
