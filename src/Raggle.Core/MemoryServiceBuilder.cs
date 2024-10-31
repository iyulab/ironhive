using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raggle.Core;

public class MemoryServiceBuilder
{
    private readonly IServiceCollection _services;

    public MemoryServiceBuilder(IServiceCollection? services)
    {
        _services = services ?? new ServiceCollection();
        throw new NotImplementedException();
    }

    public IMemoryService Build()
    {
        throw new NotImplementedException();
    }
}
