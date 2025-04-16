using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenFGADemo.Services;

namespace OpenFGADemo;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FgaAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _resource;
    private readonly string[] _permissions;
    private readonly FgaResultOperator _resultOperator = FgaResultOperator.Or;

    /// <summary>
    /// Initializes a new instance of the <see cref="FgaAuthorizeAttribute"/> class with Or operator.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="permissions">The permissions.</param>
    public FgaAuthorizeAttribute(string resource, params string[] permissions)
    {
        _resource = resource;
        _permissions = permissions;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FgaAuthorizeAttribute"/> class with And operator.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="permissions">The permissions.</param>
    public FgaAuthorizeAttribute(string resource, bool isAndOperator = false, params string[] permissions)
    {
        _resource = resource;
        _permissions = permissions;
        _resultOperator = isAndOperator ? FgaResultOperator.And : FgaResultOperator.Or;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User.Identity?.Name;

        if (string.IsNullOrEmpty(user))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authorizeService = context.HttpContext.RequestServices.GetService<IAuthorizeService>();
        if (authorizeService == null)
        {
            context.Result = new StatusCodeResult(500); // Internal Server Error
            return;
        }
        var hasAccessArray = await authorizeService.CheckAccess(user, _resource, _permissions);
        if ((_resultOperator == FgaResultOperator.Or && hasAccessArray.All(x => !x))
            || (_resultOperator == FgaResultOperator.And && hasAccessArray.Any(x => !x)))
        {
            context.Result = new ForbidResult();
            // OR : false false false
            // AND : true true false
        }
        //else cases : 
        // OR : true false false
        // AND : true true true
    }
}

public enum FgaResultOperator
{
    Or,
    And
}