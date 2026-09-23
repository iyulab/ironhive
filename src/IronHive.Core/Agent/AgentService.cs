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

    /// <summary>키 검사용 — 맵으로만 읽는다(네이밍 규칙 없이 원래 키 그대로).</summary>
    private static readonly IDeserializer YamlMapDeserializer = new DeserializerBuilder().Build();

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

        CheckJsonKeys(json);

        AgentConfig? config;

        // Try parsing as root object with "agent" property first
        try
        {
            var root = JsonSerializer.Deserialize<AgentConfigRoot>(json, JsonOptions);
            if (root?.Agent != null && (
                !string.IsNullOrWhiteSpace(root.Agent.Provider) ||
                !string.IsNullOrWhiteSpace(root.Agent.Model)))
            {
                config = root.Agent;
            }
            else
            {
                // Try parsing as direct AgentConfig
                config = JsonSerializer.Deserialize<AgentConfig>(json, JsonOptions);
            }
        }
        catch (JsonException)
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

        var tomlModel = TomlSerializer.Deserialize<TomlTable>(toml)
            ?? throw new ArgumentException("Failed to parse TOML string.", nameof(toml));
        var agentTable = tomlModel.TryGetValue("agent", out var agentValue) && agentValue is TomlTable nested
            ? nested
            : tomlModel;
        if (!ReferenceEquals(agentTable, tomlModel))
            AgentConfigKeys.Check(tomlModel.Keys.Where(k => k != "agent"), null, "TOML");
        AgentConfigKeys.Check(
            agentTable.Keys,
            agentTable.TryGetValue("parameters", out var p) && p is TomlTable pt ? pt.Keys : null,
            "TOML",
            allowTomlAliases: true);

        var config = ParseTomlToConfig(tomlModel);

        return CreateAgentFromConfig(config);
    }

    /// <inheritdoc />
    public IAgent CreateAgentFromYaml(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            throw new ArgumentException("YAML string cannot be null or empty.", nameof(yaml));

        CheckYamlKeys(yaml);

        AgentConfig? config;

        // Try parsing as root object with "agent" property first
        try
        {
            var root = YamlDeserializer.Deserialize<AgentConfigRoot>(yaml);
            if (root?.Agent != null && (
                !string.IsNullOrWhiteSpace(root.Agent.Provider) ||
                !string.IsNullOrWhiteSpace(root.Agent.Model)))
            {
                config = root.Agent;
            }
            else
            {
                // Try parsing as direct AgentConfig
                config = YamlDeserializer.Deserialize<AgentConfig>(yaml);
            }
        }
        catch (Exception)
        {
            // Fallback to direct parsing
            config = YamlDeserializer.Deserialize<AgentConfig>(yaml);
        }

        if (config == null)
            throw new ArgumentException("Failed to parse YAML as AgentConfig.", nameof(yaml));

        return CreateAgentFromConfig(config);
    }

    private static void CheckJsonKeys(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return;

        var root = document.RootElement;
        var agent = root.EnumerateObject()
            .FirstOrDefault(p => string.Equals(p.Name, "agent", StringComparison.OrdinalIgnoreCase));
        var nested = agent.Value.ValueKind == JsonValueKind.Object;
        var agentElement = nested ? agent.Value : root;
        var comparer = StringComparer.OrdinalIgnoreCase;

        if (nested)
        {
            AgentConfigKeys.Check(
                root.EnumerateObject().Select(p => p.Name).Where(n => !comparer.Equals(n, "agent")),
                null, "JSON", comparer: comparer);
        }

        var parameters = agentElement.EnumerateObject()
            .FirstOrDefault(p => comparer.Equals(p.Name, "parameters"));
        AgentConfigKeys.Check(
            agentElement.EnumerateObject().Select(p => p.Name),
            parameters.Value.ValueKind == JsonValueKind.Object
                ? parameters.Value.EnumerateObject().Select(p => p.Name)
                : null,
            "JSON",
            comparer: comparer);
    }

    private static void CheckYamlKeys(string yaml)
    {
        if (YamlMapDeserializer.Deserialize<Dictionary<object, object?>>(yaml) is not { } root)
            return;

        var agent = root.TryGetValue("agent", out var nested) && nested is Dictionary<object, object?> map
            ? map
            : root;
        if (!ReferenceEquals(agent, root))
            AgentConfigKeys.Check(root.Keys.Select(k => k.ToString()!).Where(k => k != "agent"), null, "YAML");

        AgentConfigKeys.Check(
            agent.Keys.Select(k => k.ToString()!),
            agent.TryGetValue("parameters", out var p) && p is Dictionary<object, object?> parameters
                ? parameters.Keys.Select(k => k.ToString()!)
                : null,
            "YAML");
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
            Instructions = config.Instructions,
            MaxTokens = config.Parameters?.MaxTokens,
            Temperature = config.Parameters?.Temperature,
            TopP = config.Parameters?.TopP,
            TopK = config.Parameters?.TopK,
            StopSequences = config.Parameters?.StopSequences
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
