using Microsoft.Extensions.DependencyInjection;

namespace System;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// 지정된 <paramref name="serviceKey"/> 에 해당하는 서비스 인스턴스를 <see cref="IServiceProvider"/> 로부터 가져오거나,
    /// 존재하지 않는 경우 새 인스턴스를 생성하여 반환합니다.
    /// </summary>
    /// <typeparam name="T">
    /// 가져오거나 생성할 서비스의 형식입니다. 
    /// </typeparam>
    /// <param name="serviceKey">
    /// 등록된 서비스 인스턴스를 식별하기 위한 키. 
    /// </param>
    /// <returns>
    /// 지정된 키에 해당하는 서비스 인스턴스,
    /// 또는 서비스가 등록되지 않은 경우 새로 생성된 인스턴스.
    /// </returns>
    public static T GetKeyedServiceOrCreate<T>(this IServiceProvider? services, object? serviceKey)
        where T : class
    {
        if (services is null)
        {
            // 기본 생성자 확인
            var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor is not null)
            {
                return (T)Activator.CreateInstance(typeof(T))!;
            }

            throw new InvalidOperationException(
                $"타입 '{typeof(T).FullName}' 은 기본 생성자가 없고 ServiceProvider 없이 생성할 수 없습니다.");
        }
        else
        {
            return services.GetKeyedService<T>(serviceKey)
                   ?? ActivatorUtilities.CreateInstance<T>(services);
        }
    }
}
