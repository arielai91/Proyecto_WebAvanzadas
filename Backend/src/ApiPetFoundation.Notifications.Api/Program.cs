using ApiPetFoundation.Application;
using ApiPetFoundation.Infrastructure;
using ApiPetFoundation.Notifications.Api.BackgroundServices;
using ApiPetFoundation.Infrastructure.Configuration;
using ApiPetFoundation.Notifications.Api.Hubs;
using ApiPetFoundation.Notifications.Api.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs - HTTP and HTTPS in development
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5003); // HTTP
    options.ListenLocalhost(5004, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, DomainUserIdProvider>();
builder.Services.AddHealthChecks();
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<RabbitMqPetEventSubscriber>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5000",
                "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Disabled HTTPS redirection in development to avoid connection issues
// app.UseHttpsRedirection();
app.UseCors("AllowGateway");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

await app.RunAsync();
