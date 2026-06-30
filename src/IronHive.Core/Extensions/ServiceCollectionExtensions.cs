using IronHive.Abstractions;
using IronHive.Abstractions.Workflow;
using IronHive.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Service Collection에 기존 IHiveService 인스턴스를 등록합니다.
    /// </summary>
    public static IServiceCollection AddHiveService(
        this IServiceCollection services,
        IHiveService service,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(IHiveService), _ => service, lifetime));
        return services;
    }

    /// <summary>
    /// Service Collection에 IronHive 서비스를 팩토리 방식으로 등록합니다.
    /// 기본 lifetime은 Scoped입니다 — 요청마다 IServiceProvider가 주입되어 DbContext 등 Scoped 서비스를 도구 실행에서 사용할 수 있습니다.
    /// </summary>
    public static IServiceCollection AddHiveService(
        this IServiceCollection services,
        Func<IHiveServiceBuilder, IServiceProvider, IHiveService> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Add(new ServiceDescriptor(typeof(IHiveService), sp =>
        {
            var builder = new HiveServiceBuilder();
            return configure(builder, sp);
        }, lifetime));
        return services;
    }

    /// <summary>
    /// DI를 위한 워크플로우 스텝을 등록합니다.
    /// </summary>
    public static IServiceCollection AddWorkflowStep<TStep>(this IServiceCollection services, string name)
        where TStep : class, IWorkflowStep
    {
        services.AddKeyedTransient<IWorkflowStep, TStep>(name);
        return services;
    }
}
