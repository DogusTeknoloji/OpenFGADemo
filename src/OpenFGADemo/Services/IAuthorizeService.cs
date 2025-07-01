using Microsoft.AspNetCore.Authorization;
using OpenFGADemo.Controllers;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenFGADemo.Services;

public interface IAuthorizeService
{
    Task<bool> CheckAccess(string userId, string documentId, string permission);

    Task<bool[]> CheckAccess(string userId, string documentId, params string[] permissions);
}

public interface IDemoService
{
    void CreateStore();

    string GetStoreName(int id);

    Task CreateStoreAsync();

    [FgaAuthorize("DemoWebApi", Constantants.Owner, Constantants.Admin)]
    Task<string> GetStoreNameAsync(int id);
}

public class DemoService : IDemoService
{
    // AOP : Aspect Oriented Programming
    [Authorize]
    [FgaAuthorize("DemoWebApi", Constantants.Owner, Constantants.Admin)]
    public void CreateStore()
    {
        Console.WriteLine("Demo");
    }

    public Task CreateStoreAsync()
    {
        Console.WriteLine("Demo");
        return Task.CompletedTask;
    }

    public string GetStoreName(int id)
    {
        Console.WriteLine("Demo");
        return "Demo";
    }

    public Task<string> GetStoreNameAsync(int id)
    {
        Console.WriteLine("Demo");
        return Task.FromResult("Demo");
    }
}

public sealed class DemoService__Proxy : ProxyBase<IDemoService, DemoService>, IDemoService
{
    public DemoService__Proxy(DemoService demoService, IEnumerable<IInterceptor<IDemoService, DemoService>> interceptors)
        : base(demoService, interceptors)
    {
    }

    public void CreateStore()
    {
        RunForAction(
            i => i.CreateStore(), // interface method
            c => c.CreateStore(), // class method
            m => m.CreateStore(), // runner method
            []
        );
    }

    public Task CreateStoreAsync()
    {
        return RunForActionAsync(
            i => i.CreateStoreAsync(), // interface method
            c => c.CreateStoreAsync(), // class method
            m => m.CreateStoreAsync(), // runner method
            []
        );
    }

    public string GetStoreName(int id)
    {
        return RunForFunction(
            i => i.GetStoreName(id), // interface method
            c => c.GetStoreName(id), // class method
            m => m.GetStoreName(id), // runner method
            [id]
        );
    }

    public Task<string> GetStoreNameAsync(int id)
    {
        return RunForFunctionAsync(
            i => i.GetStoreNameAsync(id), // interface method
            c => c.GetStoreNameAsync(id), // class method
            m => m.GetStoreNameAsync(id), // runner method
            [id]
        );
    }
}

public static class DynamicProxyServiceExtensions
{
    private static readonly Dictionary<Type, (Type ProxyClassType, List<Type> Interceptors)> _proxies = new();

    public static IServiceCollection AddDynamicProxyService(this IServiceCollection services)
    {
        services.AddSingleton(_proxies);
        return services;
    }

    public static IServiceCollection AddTransientWithInterceptors<TInterface, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddTransient<TImplementation>();
        Type proxyClassType = Prepare<TInterface, TImplementation>(services, interceptorTypes);

        services.AddTransient(typeof(TInterface), proxyClassType);
        return services;
    }

    public static IServiceCollection AddScopedWithInterceptors<TInterface, TImplementation>(
        this IServiceCollection services,
        params Type[] interceptorTypes)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TImplementation>();
        Type proxyClassType = Prepare<TInterface, TImplementation>(services, interceptorTypes);

        services.AddScoped(typeof(TInterface), proxyClassType);
        return services;
    }

    public static IServiceCollection AddSingletonWithInterceptors<TInterface, TImplementation>(this IServiceCollection services, params Type[] interceptorTypes)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TImplementation>();
        Type proxyClassType = Prepare<TInterface, TImplementation>(services, interceptorTypes);

        services.AddSingleton(typeof(TInterface), proxyClassType);
        return services;
    }


    private static Type Prepare<TInterface, TImplementation>(IServiceCollection services, Type[] interceptorTypes)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var proxyType = typeof(TImplementation).Name + "__Proxy";
        var proxyClassType = typeof(TImplementation).Assembly.GetTypes().FirstOrDefault(t => t.Name == proxyType) ?? throw new InvalidOperationException($"Proxy class {proxyType} not found.");

        if (_proxies.ContainsKey(proxyClassType)) throw new InvalidOperationException($"Proxy class {proxyType} already registered.");

        var intList = interceptorTypes
            .Select(x => x.IsGenericTypeDefinition ? x : x.GetGenericTypeDefinition())
            .Select(x => x.MakeGenericType(typeof(TInterface), typeof(TImplementation)))
            .ToList();

        _proxies[proxyClassType] = (proxyClassType, intList);
        var interceptorType = typeof(IInterceptor<TInterface, TImplementation>);
        foreach (var intType in intList)
        {
            services.AddTransient(interceptorType, intType);
        }

        return proxyClassType;
    }
}

public class ProxyBase<TInterface, TImplementation>(TImplementation implementation, IEnumerable<IInterceptor<TInterface, TImplementation>> interceptors)
    where TInterface : class
    where TImplementation : class, TInterface
{
    protected readonly TImplementation _implementation = implementation;
    protected readonly IReadOnlyList<IInterceptor<TInterface, TImplementation>> _interceptors = [.. interceptors];

    protected TResult RunForFunction<TResult>(
        Expression<Func<TInterface, TResult>> interfaceMethodExpression,
        Expression<Func<TImplementation, TResult>> implementationMethodExpression,
        Func<TImplementation, TResult> implementationMethod,
        IReadOnlyList<object> parameters)
    {
        if (_interceptors.Count == 0)
        {
            return implementationMethod(_implementation);
        }

        var context = new InterceptorContext<TInterface, TImplementation>(
            _interceptors,
            _implementation,
            interfaceMethodExpression.GetMethodInfo(),
            implementationMethodExpression.GetMethodInfo(),
            parameters: parameters,
            implementationRunner: () => implementationMethod(_implementation));

        context.Proceed();

        if (context.ReturnValue is TResult result)
        {
            return result;
        }

#pragma warning disable CS8603 // Possible null reference return.
        return default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    protected async Task<TResult> RunForFunctionAsync<TResult>(
        Expression<Func<TInterface, Task<TResult>>> interfaceMethodExpression,
        Expression<Func<TImplementation, Task<TResult>>> implementationMethodExpression,
        Func<TImplementation, Task<TResult>> implementationMethod,
        IReadOnlyList<object> parameters)
    {
        if (_interceptors.Count == 0)
        {
            return await implementationMethod(_implementation);
        }

        var context = new InterceptorContext<TInterface, TImplementation>(
            _interceptors,
            _implementation,
            interfaceMethodExpression.GetMethodInfo(),
            implementationMethodExpression.GetMethodInfo(),
            parameters: parameters,
            implementationRunner: () => implementationMethod(_implementation));

        await context.ProceedAsync();

        if (context.ReturnValue is Task<TResult> resultAsync)
        {
            return await resultAsync;
        }

        if (context.ReturnValue is TResult result)
        {
            return result;
        }

#pragma warning disable CS8603 // Possible null reference return.
        return default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    protected void RunForAction(
        Expression<Action<TInterface>> interfaceMethodExpression,
        Expression<Action<TImplementation>> implementationMethodExpression,
        Action<TImplementation> implementationMethod,
        IReadOnlyList<object> parameters)
    {
        if (_interceptors.Count == 0)
        {
            implementationMethod(_implementation);
            return;
        }

        var context = new InterceptorContext<TInterface, TImplementation>(
            _interceptors,
            _implementation,
            interfaceMethodExpression.GetMethodInfo(),
            implementationMethodExpression.GetMethodInfo(),
            parameters,
            () =>
            {
                implementationMethod(_implementation);
                return null;
            });

        context.Proceed();
    }

    protected async Task RunForActionAsync(
        Expression<Func<TImplementation, Task>> interfaceMethodExpression,
        Expression<Func<TImplementation, Task>> implementationMethodExpression,
        Func<TImplementation, Task> implementationMethod,
        IReadOnlyList<object> parameters)
    {
        if (_interceptors.Count == 0)
        {
            await implementationMethod(_implementation);
            return;
        }

        var context = new InterceptorContext<TInterface, TImplementation>(
            _interceptors,
            _implementation,
            interfaceMethodExpression.GetMethodInfo(),
            implementationMethodExpression.GetMethodInfo(),
            parameters,
            () =>
            {
                implementationMethod(_implementation);
                return null;
            });

        await context.ProceedAsync();
    }
}

public interface IInterceptor<TInterface, TImplementation>
    where TImplementation : class, TInterface
{
    void Intercept(IInterceptorContext<TInterface, TImplementation> context);

    Task InterceptAsync(IInterceptorContext<TInterface, TImplementation> context);
}

public interface IInterceptorContext<TInterface, TImplementation>
    where TImplementation : class, TInterface
{
    bool IsAsync { get; }

    TInterface InterfaceRef { get; }
    TImplementation ImplementationRef { get; }

    MethodInfo InterfaceMethodInfo { get; }

    MethodInfo ImplementationMethodInfo { get; }

    IReadOnlyList<object> Parameters { get; }

    object? ReturnValue { get; set; }

    void Proceed();

    Task ProceedAsync();
}

internal static class TypeExtensions
{
    public static bool IsAsyncType(this Type type)
    {
        return type == typeof(Task) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>));
    }

    public static MethodInfo GetMethodInfo<T>(string name, int parameterCount = 0, bool isAsyncType = false, int genericParameterCount = 0, Func<IEnumerable<MethodInfo>, IEnumerable<MethodInfo>>? advantageFilter = null)
    {
        var n = name;
        if (!name.Contains('`') && genericParameterCount > 0)
        {
            n = name + "`" + genericParameterCount;
        }
        var t = typeof(T);
        var methodInfos = (isAsyncType
            ? t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == n && IsAsyncType(x.ReturnType))
            : t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == n))
            .ToArray();

        if (methodInfos.Length == 0)
        {
            throw new InvalidOperationException($"Method {n} not found.");
        }
        if (methodInfos.Length == 1)
        {
            return methodInfos[0];
        }
        if (parameterCount == 0)
            return methodInfos.FirstOrDefault(x => x.GetParameters().Length == 0) ?? throw new InvalidOperationException($"Method {n} not found.");

        if (advantageFilter == null)
        {
            throw new InvalidOperationException($"Method {n} has more then one instance.");
        }

        var mt2 = advantageFilter(methodInfos.Where(x => x.GetParameters().Length == parameterCount)).ToArray();

        if (mt2.Length == 0)
        {
            throw new InvalidOperationException($"Method {n} not found.");
        }
        if (mt2.Length == 1)
        {
            return mt2[0];
        }

        throw new InvalidOperationException($"Method {n} has more then one instance.");
    }

    public static MethodInfo GetMethodInfo<TService>(this Expression<Action<TService>> expr)
    {
        if (expr.Body is MethodCallExpression call)
            return call.Method;
        throw new ArgumentException("Expression must be a method call", nameof(expr));
    }

    public static MethodInfo GetMethodInfo<TService, TResult>(this Expression<Func<TService, TResult>> expr)
    {
        if (expr.Body is MethodCallExpression call)
            return call.Method;
        throw new ArgumentException("Expression must be a method call", nameof(expr));
    }
}

internal class InterceptorContext<TInterface, TImplementation>
    : IInterceptorContext<TInterface, TImplementation>
    where TImplementation : class, TInterface
{
    private readonly IReadOnlyList<IInterceptor<TInterface, TImplementation>> _interceptors;
    private int runningIndex = -1;
    private readonly Func<object?> _implementationRunner;

    public InterceptorContext(
        IReadOnlyList<IInterceptor<TInterface, TImplementation>> interceptors,
        TImplementation implementation,
        MethodInfo interfaceMethodInfo,
        MethodInfo implementationMethodInfo,
        IReadOnlyList<object> parameters,
        Func<object?> implementationRunner)
    {
        _interceptors = interceptors;
        ImplementationRef = implementation;
        ImplementationMethodInfo = implementationMethodInfo;
        IsAsync = implementationMethodInfo.ReturnType.IsAsyncType();
        InterfaceMethodInfo = interfaceMethodInfo;
        Parameters = parameters;
        _implementationRunner = implementationRunner;
    }

    public bool IsAsync { get; }
    public bool IsReturnVoid => ImplementationMethodInfo.ReturnType == typeof(void) || ImplementationMethodInfo.ReturnType == typeof(Task);
    public TInterface InterfaceRef => ImplementationRef;
    public TImplementation ImplementationRef { get; }
    public MethodInfo InterfaceMethodInfo { get; }
    public MethodInfo ImplementationMethodInfo { get; }
    public IReadOnlyList<object> Parameters { get; }
    public object? ReturnValue { get; set; } = null;

    public void Proceed()
    {
        runningIndex++;
        if (runningIndex > _interceptors.Count)
        {
            return;
        }
        if (runningIndex == _interceptors.Count)
        {
            ReturnValue = _implementationRunner();
            return;
        }
        var interceptor = _interceptors[runningIndex];
        interceptor.Intercept(this);
    }

    public async Task ProceedAsync()
    {
        runningIndex++;
        if (runningIndex > _interceptors.Count)
        {
            return;
        }
        if (runningIndex == _interceptors.Count)
        {
            var rv = _implementationRunner();
            ReturnValue = rv;
            if (rv is Task task)
            {
                await task;
            }

            return;
        }
        var interceptor = _interceptors[runningIndex];
        await interceptor.InterceptAsync(this);
    }
}

public class LoggingInterceptor<TInterface, TImplementation> : IInterceptor<TInterface, TImplementation>
        where TImplementation : class, TInterface

{
    public void Intercept(IInterceptorContext<TInterface, TImplementation> context)
    {
        Console.WriteLine($"Intercepting {context.InterfaceMethodInfo.Name} with parameters: {string.Join(", ", context.Parameters)}");
        context.Proceed();
        Console.WriteLine($"Return value: {context.ReturnValue}");
    }

    public async Task InterceptAsync(IInterceptorContext<TInterface, TImplementation> context)
    {
        Console.WriteLine($"Intercepting {context.InterfaceMethodInfo.Name} with parameters: {string.Join(", ", context.Parameters)}");
        await context.ProceedAsync();
        Console.WriteLine($"Return value: {context.ReturnValue}");
    }
}
