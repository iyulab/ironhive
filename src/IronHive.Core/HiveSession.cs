using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion.Messages;

namespace IronHive.Core;

public class HiveSession : IHiveSession
{
    private readonly IDictionary<string, IHiveAgent> _agents = new Dictionary<string, IHiveAgent>();

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public int? LastSummarizedIndex { get; set; }

    public MessageCollection Messages { get; set; } = new();

    public int? TotalTokens { get; set; }

    public int MaxToolAttempts { get; set; } = 3;

    //public bool AutoUpdate { get; set; } = true;

    public HiveSession()
    { }

    public HiveSession(IEnumerable<IHiveAgent> agents)
    {
        _agents = agents.ToDictionary(a => a.Name, a => a);
    }

    public void AddAgent(IHiveAgent agent)
    {
        _agents.Add(agent.Name, agent);
    }

    public void RemoveAgent(string name)
    {
        _agents.Remove(name);
    }
}
