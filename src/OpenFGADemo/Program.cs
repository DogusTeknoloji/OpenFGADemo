using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


// OpenFGA configuration
builder.Services.AddSingleton(new OpenFga.Sdk.Client.ClientConfiguration { 
    //ApiScheme = "http",
    //ApiHost = "localhost:8080"
    ApiUrl = "http://localhost:8080",
    StoreId = "01JPMEMAWSXD1VPBWKSBCVQ4KH"
});

builder.Services.AddSingleton<OpenFga.Sdk.Client.OpenFgaClient>(sp =>
{
    var config = sp.GetRequiredService<OpenFga.Sdk.Client.ClientConfiguration>();
    return new OpenFga.Sdk.Client.OpenFgaClient(config);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
