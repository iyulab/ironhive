namespace IronHive.Abstractions.ChatCompletion;

public interface IChatCompletionServiceBuilder
{
    IChatCompletionServiceBuilder AddConnector(string key, IChatCompletionConnector connector);

    IChatCompletionServiceBuilder AddTool<TService>(string key, TService? implementation = null) where TService : class;

    IChatCompletionServiceBuilder WithParser(IServiceModelParser parser);

    IChatCompletionService Build();
}
