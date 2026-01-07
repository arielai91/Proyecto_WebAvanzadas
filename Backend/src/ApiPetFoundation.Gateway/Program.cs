using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:5000");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseRouting();

app.MapReverseProxy();
app.MapHealthChecks("/health");

await app.RunAsync();
