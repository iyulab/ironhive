using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Core;

public class RaggleBuilder
{
    public IServiceCollection Services { get; set; } = new ServiceCollection();

    public void AddSingleton()
    {

    }

    public void AddTransient()
    {

    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(ServiceDescriptor item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }
}
