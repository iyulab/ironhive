using IronHive.Abstractions;
using IronHive.Core;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        var builder = new HiveServiceBuilder(services);
        return builder;
    }

    public static IHiveServiceBuilder AddHiveServiceDefault(this IServiceCollection services)
    {
        var builder = services.AddHiveServiceCore();
        builder.AddDefaultFileDecoders();
        builder.AddDefaultPipelineHandlers();
        return builder;
    }

    public static IServiceCollection AddPipelineWorkerService(this IServiceCollection services)
    {
        //services.AddHostedService<PipelineWorker>();
        return services;
    }
}

//public class WorkingClass : IHostedService
//{
//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        return Task.CompletedTask;
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        return Task.CompletedTask;
//    }
//}