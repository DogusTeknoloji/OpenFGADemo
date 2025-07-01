namespace OpenFGADemo.Services;

public class FgaCheckInterceptor<TInterface, TImplementation>(
    SessionInfo sessionInfo,
    IAuthorizeService authorizeService)
    : IInterceptor<TInterface, TImplementation>
    where TImplementation : class, TInterface

{
    public void Intercept(IInterceptorContext<TInterface, TImplementation> context)
    {
        if ((context.InterfaceMethodInfo
            .GetCustomAttributes(typeof(FgaAuthorizeAttribute), true))
            .Union(context.ImplementationMethodInfo
                .GetCustomAttributes(typeof(FgaAuthorizeAttribute), true))
            .FirstOrDefault() is not FgaAuthorizeAttribute attribute)
        {
            context.Proceed();
            return;
        }

        if (string.IsNullOrEmpty(sessionInfo.UserName))
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var hasAccessArray = authorizeService.CheckAccess(sessionInfo.UserName, attribute.Resource, attribute.Permissions).GetAwaiter().GetResult();
        if ((attribute.ResultOperator == FgaResultOperator.Or && hasAccessArray.All(x => !x))
            || (attribute.ResultOperator == FgaResultOperator.And && hasAccessArray.Any(x => !x)))
        {
            throw new InvalidOperationException("User is not authorized.");
            // OR : false false false
            // AND : true true false
        }

        context.Proceed();
    }

    public async Task InterceptAsync(IInterceptorContext<TInterface, TImplementation> context)
    {
        if ((context.InterfaceMethodInfo
            .GetCustomAttributes(typeof(FgaAuthorizeAttribute), true))
            .Union(context.ImplementationMethodInfo
                .GetCustomAttributes(typeof(FgaAuthorizeAttribute), true))
            .FirstOrDefault() is not FgaAuthorizeAttribute attribute)
        {
            await context.ProceedAsync();
            return;
        }

        if (string.IsNullOrEmpty(sessionInfo.UserName))
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var hasAccessArray = await authorizeService.CheckAccess(sessionInfo.UserName, attribute.Resource, attribute.Permissions);
        if ((attribute.ResultOperator == FgaResultOperator.Or && hasAccessArray.All(x => !x))
            || (attribute.ResultOperator == FgaResultOperator.And && hasAccessArray.Any(x => !x)))
        {
            throw new InvalidOperationException("User is not authorized.");
            // OR : false false false
            // AND : true true false
        }

        await context.ProceedAsync();
    }
}

public class SessionInfo
{
    public Guid SessionId { get; } = Guid.CreateVersion7();

    public string? UserName { get; set; } = "admin";
}