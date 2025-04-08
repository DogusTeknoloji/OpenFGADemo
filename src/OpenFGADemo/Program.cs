using Microsoft.AspNetCore.Authentication;
using OpenFGADemo.Services;
using Scalar.AspNetCore;

namespace OpenFGADemo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        // OpenFGA configuration
        builder.Services.AddSingleton(new OpenFga.Sdk.Client.ClientConfiguration
        {
            //ApiScheme = "http",
            //ApiHost = "localhost:8080"
            ApiUrl = "http://localhost:8080",
            StoreId = "01JPMEMAWSXD1VPBWKSBCVQ4KH"
        });

        builder.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<OpenFga.Sdk.Client.ClientConfiguration>();
            return new OpenFga.Sdk.Client.OpenFgaClient(config);
        });

        // Add Basic Authentication
        builder.Services.AddAuthentication("BasicAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

        builder.Services.AddAuthorization();

        builder.Services.AddTransient<IAuthorizeService, AuthorizeService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        // Use authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}