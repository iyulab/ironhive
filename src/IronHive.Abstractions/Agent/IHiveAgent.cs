using IronHive.Abstractions.Message;

namespace IronHive.Abstractions.Agent;

/// <summary>
/// Represents an agent in the Hive system.
/// </summary>
public interface IHiveAgent
{
    /// <summary>
    /// the model provider service key of the agent.
    /// </summary>
    string Provider { get; set; }

    /// <summary>
    /// the model name used by the agent.
    /// </summary>
    string Model { get; set; }

    /// <summary>
    /// the name of the agent.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// the description of the agent.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// instructions for the agent.
    /// </summary>
    string? Instructions { get; set; }

    /// <summary>
    /// the tools available to the agent.
    /// key is the service key of the tool handler.
    /// value is the tool handler options object.
    /// </summary>
    IDictionary<string, object?>? Tools { get; set; }

    /// <summary>
    /// the parameters for text generation inference.
    /// </summary>
    MessageGenerationParameters? Parameters { get; set; }
}
