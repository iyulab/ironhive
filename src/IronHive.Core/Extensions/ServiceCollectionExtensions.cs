using IronHive.Abstractions;
using IronHive.Abstractions.Workflow;
using IronHive.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Service Collection에 IronHive 서비스를 등록합니다.
    /// 호스트 DI 컨테이너에 <see cref="IHiveService"/>를 싱글톤으로 등록하고,
    /// 동일 <see cref="IServiceCollection"/>을 사용하는 빌더를 반환합니다.
    /// </summary>
    /// <remarks>
    /// 이 메서드를 사용할 때 <c>new HiveServiceBuilder()</c>로 별도 빌더를 생성하지 마세요.
    /// 두 경로를 혼용하면 독립된 서비스 인스턴스가 생겨 레지스트리 불일치가 발생합니다.
    /// </remarks>
    public static IHiveServiceBuilder AddHiveServiceCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IHiveService, HiveService>();
        var builder = new HiveServiceBuilder(services);
        return builder;
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
