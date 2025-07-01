using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using OpenFGADemo.Services;

namespace OpenFGADemo.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private static readonly List<DemoDto> list =
    [
        new DemoDto(1, "Alice", DateTimeOffset.UtcNow.AddDays(-10)),
        new DemoDto(2, "Bob", DateTimeOffset.UtcNow.AddDays(-9)),
        new DemoDto(3, "Charlie", DateTimeOffset.UtcNow.AddDays(-8)),
        new DemoDto(4, "Diana", DateTimeOffset.UtcNow.AddDays(-7)),
        new DemoDto(5, "Eve", DateTimeOffset.UtcNow.AddDays(-6)),
        new DemoDto(6, "Frank", DateTimeOffset.UtcNow.AddDays(-5)),
        new DemoDto(7, "Grace", DateTimeOffset.UtcNow.AddDays(-4)),
        new DemoDto(8, "Hank", DateTimeOffset.UtcNow.AddDays(-3)),
        new DemoDto(9, "Ivy", DateTimeOffset.UtcNow.AddDays(-2)),
        new DemoDto(10, "Jack", DateTimeOffset.UtcNow.AddDays(-1))
    ];

    private readonly IAuthorizeService _authorizeService;

    public DemoController(IAuthorizeService authorizeService)
    {
        _authorizeService = authorizeService;
    }

    [HttpGet]
    [Authorize]
    [FgaAuthorize("DemoWebApi", Constantants.Owner, Constantants.Admin)]
    public async Task<IActionResult> List()
    {
        //if(!User.Identity?.IsAuthenticated ?? true)
        //{
        //    return Unauthorized();
        //}
        //var accessCheck = await _authorizeService.CheckAccess(User.Identity?.Name ?? string.Empty, "DemoWebApi", "owner");
        //if (!accessCheck)
        //{
        //    return Forbid();
        //}

        return Ok(list);
    }

    [HttpGet("[action]")]
    [Authorize]
    //[FgaAuthorize("DemoWebApi", Constantants.Owner, Constantants.Admin)]
    public async Task<IActionResult> Demo([FromServices] IDemoService demoService)
    {
        //if(!User.Identity?.IsAuthenticated ?? true)
        //{
        //    return Unauthorized();
        //}
        //var accessCheck = await _authorizeService.CheckAccess(User.Identity?.Name ?? string.Empty, "DemoWebApi", "owner");
        //if (!accessCheck)
        //{
        //    return Forbid();
        //}

        var a = await demoService.GetStoreNameAsync(1);

        return Ok(list);
    }

    public record DemoDto(int Id, string Name, DateTimeOffset CreatedAt);
}

public static class Constantants
{
    public const string Owner = "owner";
    public const string Admin = "admin";
}