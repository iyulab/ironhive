using System.Text.Json;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using Tomlyn;
using Tomlyn.Model;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IronHive.Core.Agent;

/// <inheritdoc />
public class AgentService : IAgentService
{
    private readonly IMessageService _messages;

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgentService(IMessageService message)
    {
        _messages = message;
    }

    /// <inheritdoc />
    public IAgent CreateAgent(Action<AgentConfig> configure)
    {
        var config = new AgentConfig();
        configure(config);
        return CreateAgentFromConfig(config);
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));

        AgentConfig? config;

        // Try parsing as root object with "agent" property first
        try
        {
            var root = JsonSerializer.Deserialize<AgentConfigRoot>(json, JsonOptions);
            if (root?.Agent != null && !string.IsNullOrWhiteSpace(root.Agent.Name))
            {
                config = root.Agent;
            }
            else
            {
                // Try parsing as direct AgentConfig
                config = JsonSerializer.Deserialize<AgentConfig>(json, JsonOptions);
            }
        }
        catch
        {
            // Fallback to direct parsing
            config = JsonSerializer.Deserialize<AgentConfig>(json, JsonOptions);
        }

        if (config == null)
            throw new ArgumentException("Failed to parse JSON as AgentConfig.", nameof(json));

        return CreateAgentFromConfig(config);
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromToml(string toml)
    {
        if (string.IsNullOrWhiteSpace(toml))
            throw new ArgumentException("TOML string cannot be null or empty.", nameof(toml));

        var tomlModel = Toml.ToModel(toml);
        var config = ParseTomlToConfig(tomlModel);

        return CreateAgentFromConfig(config);
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromYaml(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            throw new ArgumentException("YAML string cannot be null or empty.", nameof(yaml));

        AgentConfig? config;

        // Try parsing as root object with "agent" property first
        try
        {
            var root = YamlDeserializer.Deserialize<AgentConfigRoot>(yaml);
            if (root?.Agent != null && !string.IsNullOrWhiteSpace(root.Agent.Name))
            {
                config = root.Agent;
            }
            else
            {
                // Try parsing as direct AgentConfig
                config = YamlDeserializer.Deserialize<AgentConfig>(yaml);
            }
        }
        catch
        {
            // Fallback to direct parsing
            config = YamlDeserializer.Deserialize<AgentConfig>(yaml);
        }

        if (config == null)
            throw new ArgumentException("Failed to parse YAML as AgentConfig.", nameof(yaml));

        return CreateAgentFromConfig(config);
    }

    /// <summary>
    /// AgentConfig에서 BasicAgent를 생성합니다.
    /// </summary>
    private BasicAgent CreateAgentFromConfig(AgentConfig config)
    {
        config.Validate();

        return new BasicAgent(_messages)
        {
            Name = config.Name,
            Description = config.Description ?? string.Empty,
            Provider = config.Provider,
            Model = config.Model,
            Instruction = config.Instructions,
            Tools = config.ToToolItems(),
            Parameters = config.ToParameters()
        };
    }

    /// <summary>
    /// TOML 모델을 AgentConfig로 변환합니다.
    /// </summary>
    private static AgentConfig ParseTomlToConfig(TomlTable toml)
    {
        var agentTable = toml.ContainsKey("agent")
            ? (TomlTable)toml["agent"]
            : toml;

        var config = new AgentConfig
        {
            Name = GetTomlString(agentTable, "name") ?? string.Empty,
            Description = GetTomlString(agentTable, "description") ?? string.Empty,
            Provider = GetTomlString(agentTable, "provider")
                       ?? GetTomlString(agentTable, "defaultProvider") ?? string.Empty,
            Model = GetTomlString(agentTable, "model")
                    ?? GetTomlString(agentTable, "defaultModel") ?? string.Empty,
            Instructions = GetTomlString(agentTable, "instructions"),
            Tools = GetTomlStringList(agentTable, "tools"),
            ToolOptions = GetTomlToolOptions(agentTable),
            Parameters = GetTomlParameters(agentTable)
        };

        return config;
    }

    private static string? GetTomlString(TomlTable table, string key)
    {
        return table.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static List<string>? GetTomlStringList(TomlTable table, string key)
    {
        if (!table.TryGetValue(key, out var value) || value is not TomlArray array)
            return null;

        return array.Select(item => item?.ToString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
    }

    private static Dictionary<string, object?>? GetTomlToolOptions(TomlTable table)
    {
        if (!table.TryGetValue("toolOptions", out var value) || value is not TomlTable optionsTable)
            return null;

        return optionsTable.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) as Dictionary<string, object?>;
    }

    private static AgentParametersConfig? GetTomlParameters(TomlTable table)
    {
        if (!table.TryGetValue("parameters", out var value) || value is not TomlTable paramsTable)
            return null;

        return new AgentParametersConfig
        {
            MaxTokens = GetTomlInt(paramsTable, "maxTokens"),
            Temperature = GetTomlFloat(paramsTable, "temperature"),
            TopP = GetTomlFloat(paramsTable, "topP"),
            TopK = GetTomlInt(paramsTable, "topK"),
            StopSequences = GetTomlStringList(paramsTable, "stopSequences")
        };
    }

    private static int? GetTomlInt(TomlTable table, string key)
    {
        if (!table.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            long l => (int)l,
            int i => i,
            _ => null
        };
    }

    private static float? GetTomlFloat(TomlTable table, string key)
    {
        if (!table.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            double d => (float)d,
            float f => f,
            long l => l,
            int i => i,
            _ => null
        };
    }
}
