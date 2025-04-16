
using Microsoft.AspNetCore.Authorization;
using OpenFGADemo.Controllers;

namespace OpenFGADemo.Services;

public interface IAuthorizeService
{
    Task<bool> CheckAccess(string userId, string documentId, string permission);
    Task<bool[]> CheckAccess(string userId, string documentId, params string[] permissions);
}


public interface IDemoService
{
    void CreateStore();
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
}