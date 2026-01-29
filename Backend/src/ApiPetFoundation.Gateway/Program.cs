using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);
var httpsPort = builder.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT") ?? 5000;
builder.WebHost.UseUrls($"https://localhost:{httpsPort}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRouting();

app.MapReverseProxy();
app.MapHealthChecks("/health");

await app.RunAsync();
