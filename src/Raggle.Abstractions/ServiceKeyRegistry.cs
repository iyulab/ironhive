namespace Raggle.Abstractions;

public static class ServiceKeyRegistry
{
    private static Dictionary<string, Type> _inner { get; } = new();

    public static void Add<T>(string key)
    {
        _inner[key] = typeof(T);
    }

    public static Type Get(string key)
    {
        return _inner[key];
    }

    public static string Get(Type type)
    {
        return _inner.FirstOrDefault(x => x.Value == type).Key;
    }
}
