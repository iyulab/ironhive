using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Microsoft.AspNetCore.Mvc;

namespace IronHive.Core.Tools;

/// <summary>
/// ITool을 구현하는 .NET 메서드 기반 툴 구현체입니다.
/// </summary>
[JsonPolymorphicValue("function")]
public sealed class FunctionTool : ITool
{
    private readonly MethodInfo _method;
    private readonly object? _target; // 인스턴스에 바인딩된 경우

    /// <summary>
    /// 델리게이트를 받아 대상 메서드/인스턴스를 설정합니다.
    /// </summary>
    public FunctionTool(Delegate function)
    {
        _method = function.Method;
        _target = function.Target;
    }

    /// <summary>
    /// <see cref="MethodInfo"/>를 받아
    /// 순수 실행기로 동작합니다. 인스턴스가 필요하면 DI를 통해 생성합니다.
    /// </summary>
    public FunctionTool(MethodInfo method)
    {
        _method = method;
    }

    /// <inheritdoc />
    public string UniqueName => $"func_{Name}";

    /// <summary>
    /// 사용자가 정의한 도구의 이름입니다.
    /// </summary>
    public required string Name { get; init; }
    
    /// <inheritdoc />
    public required string Description { get; init; }

    /// <inheritdoc />
    public required object? Parameters { get; init; }

    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// 도구 호출의 최대 실행 시간(타임아웃)입니다.
    /// 단위는 초이며, 기본값은 60초입니다.
    /// </summary>
    public long Timeout { get; set; } = 60;

    /// <inheritdoc />
    public async Task<IToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Timeout));

        try
        {
            // 대상 인스턴스 준비
            var target = _target ?? GetOrCreateInstance(_method.DeclaringType, input.Services);

            // 인자 바인딩
            var parameters = _method.GetParameters();
            var arguments = BuildArguments(parameters, input, timeoutCts.Token);

            // 타임 아웃 레이스
            var execTask = Task.Run(async () =>
            {
                var raw = _method.Invoke(target, arguments);
                return await HandleDynamicResultAsync(raw, _method.ReturnType, timeoutCts.Token);
            }, timeoutCts.Token);

            var delayTask = Task.Delay(TimeSpan.FromSeconds(Timeout), timeoutCts.Token);
            var completed = await Task.WhenAny(execTask, delayTask).ConfigureAwait(false);
            if (completed != execTask)
            {
                // 호출자 취소가 우선
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationToken);
                
                // 타임아웃: 내부 작업에 취소 신호 전파
                timeoutCts.Cancel();
                _ = execTask.ContinueWith(t => { var _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                return new ToolTimeoutOutput();
            }

            cancellationToken.ThrowIfCancellationRequested();

            var result = await execTask.ConfigureAwait(false);
            var json = JsonSerializer.Serialize(result, JsonDefaultOptions.Options);

            // TODO: 결과 JSON 변환이 두번 발생. 최적화 필요
            return json.Length > 30_000
                ? new ToolExcessiveOutput()
                : new ToolSuccessOutput(result);
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                throw; // 호출자에서 처리
            
            return new ToolTimeoutOutput();
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            return new ToolFailureOutput(ex.InnerException);
        }
        catch (Exception ex)
        {
            return new ToolFailureOutput(ex);
        }
    }

    /// <summary>
    /// 대상 타입의 인스턴스를 DI에서 가져오거나 새로 생성합니다.
    /// </summary>
    private static object? GetOrCreateInstance(Type? type, IServiceProvider? provider)
    {
        if (type is null)
            return null;

        if (provider is null)
            return Activator.CreateInstance(type);

        return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
    }

    /// <summary>
    /// 도구 입력을 메서드 파라미터로 바인딩해 인수 배열을 구성합니다.
    /// </summary>
    private static object?[]? BuildArguments(
        ParameterInfo[] parameters,
        ToolInput input,
        CancellationToken cancellationToken)
    {
        if (parameters.Length == 0) return null;
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var name = param.Name ?? throw new InvalidOperationException("Parameter name cannot be null.");

            // 1) 특정 키 서비스 주입
            if (param.GetCustomAttribute<FromKeyedServicesAttribute>() is { } keyedAttr)
            {
                var services = input.Services?.GetKeyedServices(param.ParameterType, keyedAttr.Key);
                args[i] = param.ParameterType.IsArray ? services : services?.FirstOrDefault();
            }
            // 2) 특정 서비스 주입
            else if (param.IsDefined(typeof(FromServicesAttribute)))
            {
                var services = input.Services?.GetServices(param.ParameterType);
                args[i] = param.ParameterType.IsArray ? services : services?.FirstOrDefault();
            }
            // 3) 툴 옵션 주입
            else if (param.GetCustomAttribute<FromOptionsAttribute>() is not null && input.Options is not null)
            {
                args[i] = input.Options.ConvertTo(param.ParameterType);
            }
            // 4) 취소 토큰인 경우
            else if (param.ParameterType == typeof(CancellationToken))
            {
                args[i] = cancellationToken;
            }
            // 5) 인자가 존재하는 경우
            else if (input.TryGetValue(name, out var value))
            {
                args[i] = value.ConvertTo(param.ParameterType);
            }    
            // 6) 인자가 존재하지 않지만, 기본값이 있는 경우
            else if (param.HasDefaultValue)
            {
                args[i] = param.DefaultValue;
            }
            // 7) 인자가 존재하지 않지만, 선택적 인자인 경우
            else if (param.IsOptional)
            {
                args[i] = Type.Missing;
            }
            // 8) 인자가 존재하지 않고, 필수 인자인 경우
            else
            {
                throw new ArgumentException($"Parameter '{name}' is required");
            }
        }

        return args;
    }

    /// <summary>
    /// 메서드 결과값을 적절히 처리합니다.
    /// </summary>
    private static async Task<object?> HandleDynamicResultAsync(
        object? result,
        Type? returnType,
        CancellationToken cancellationToken)
    {
        // void / null 처리
        if (returnType == typeof(void))
        {
            return "executed done";
        }
        if (returnType is null || result is null)
        {
            return "null";
        }

        // Task / ValueTask (비제네릭)
        if (returnType == typeof(Task))
        {
            await ((Task)result).ConfigureAwait(false);
            return "executed done";
        }
        if (returnType == typeof(ValueTask))
        {
            await ((ValueTask)result).ConfigureAwait(false);
            return "executed done";
        }

        // 제네릭 반환 처리
        if (returnType.IsGenericType)
        {
            var def = returnType.GetGenericTypeDefinition();

            // Task<T> / ValueTask<T>
            if (def == typeof(Task<>))
            {
                dynamic dt = result!;
                return await dt.ConfigureAwait(false);
            }
            if (def == typeof(ValueTask<>))
            {
                dynamic dt = result!;
                return await dt.ConfigureAwait(false);
            }

            // IAsyncEnumerable<T>
            if (def == typeof(IAsyncEnumerable<>))
            {
                return await HandleAsyncEnumerableAsync(result, cancellationToken).ConfigureAwait(false);
            }
        }

        // IAsyncEnumerable<T> 구현 인터페이스인 경우
        if (returnType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)))
        {
            return await HandleAsyncEnumerableAsync(result, cancellationToken).ConfigureAwait(false);
        }

        // 커스텀 awaitable (인스턴스 메서드 GetAwaiter 존재 시)
        if (returnType.GetMethod("GetAwaiter", Type.EmptyTypes) is not null)
        {
            dynamic dt = result!;
            return await dt.ConfigureAwait(false);
        }

        // 그 외는 그대로
        return result;
    }

    /// <summary>
    /// IAsyncEnumerable<T>를 적절히 처리합니다.
    /// </summary>
    private static async Task<List<object?>> HandleAsyncEnumerableAsync(object src, CancellationToken ct)
    {
        var result = new List<object?>();

        var type = src.GetType();
        var getAsyncEnumerator =
            type.GetMethod("GetAsyncEnumerator", new[] { typeof(CancellationToken) }) ??
            type.GetMethod("GetAsyncEnumerator", Type.EmptyTypes)
            ?? throw new InvalidOperationException("IAsyncEnumerable does not have GetAsyncEnumerator.");

        var enumerator = getAsyncEnumerator.GetParameters().Length == 1
            ? getAsyncEnumerator.Invoke(src, new object[] { ct })!
            : getAsyncEnumerator.Invoke(src, null)!;

        var moveNextAsync = enumerator.GetType().GetMethod("MoveNextAsync")
            ?? throw new InvalidOperationException("Async enumerator missing MoveNextAsync.");
        var currentProp = enumerator.GetType().GetProperty("Current")
            ?? throw new InvalidOperationException("Async enumerator missing Current property.");
        var disposeAsync = enumerator.GetType().GetMethod("DisposeAsync");

        try
        {
            // MoveNextAsync: ValueTask<bool>
            while (await (ValueTask<bool>)moveNextAsync.Invoke(enumerator, null)!)
            {
                result.Add(currentProp.GetValue(enumerator));
            }
        }
        finally
        {
            if (disposeAsync is not null)
            {
                await (ValueTask)disposeAsync.Invoke(enumerator, null)!;
            }
        }

        return result;
    }
}
