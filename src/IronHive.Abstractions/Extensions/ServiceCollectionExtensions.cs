using IronHive.Abstractions.Workflow;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
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
