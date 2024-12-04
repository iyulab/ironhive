namespace Raggle.Server.Utils;

public class ServiceKeyValue<T>
{
    public string ServiceKey { get; set; } = string.Empty;

    public T Value { get; set; }

    public ServiceKeyValue()
    {
        if (typeof(T).IsClass)
            Value = Activator.CreateInstance<T>();
        else
            Value = default!;
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceKey))
            return false;

        if (Value == null)
            return false;

        return true;
    }
}
