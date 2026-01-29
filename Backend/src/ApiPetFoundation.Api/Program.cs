using ApiPetFoundation.Application;
using ApiPetFoundation.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using ApiPetFoundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using ApiPetFoundation.Api.Swagger;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs - use ASPNETCORE_URLS in Docker, HTTPS localhost in development
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    var httpsPort = builder.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT") ?? 5001;
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(httpsPort, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
}


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pet Foundation API",
        Version = "v1"
    });

    var apiXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXml);
    c.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);

    var appXml = $"{typeof(ApiPetFoundation.Application.DependencyInjection).Assembly.GetName().Name}.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXml);
    if (File.Exists(appXmlPath))
        c.IncludeXmlComments(appXmlPath);

    // Soporta JSON
    c.SupportNonNullableReferenceTypes();

    // Agregar autenticación JWT a Swagger
    var jwtSecurityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Inserta el token JWT sin comillas",

        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = "Bearer",
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<ApiPetFoundation.Api.Swagger.Examples.RegisterRequestExample>();


builder.Services.AddValidatorsFromAssembly(typeof(CreatePetRequest).Assembly);
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeed.SeedRoles(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
    var userProfileService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();
    await SeedUsers(userManager, userProfileService, builder.Configuration);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled HTTPS redirection in development to avoid 307 redirects
// app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

static async Task SeedUsers(
    UserManager<AppIdentityUser> userManager,
    IUserProfileService userProfileService,
    IConfiguration configuration)
{
    await SeedUserAsync(
        userManager,
        userProfileService,
        configuration["Seed:AdminEmail"],
        configuration["Seed:AdminPassword"],
        configuration["Seed:AdminName"] ?? "Admin",
        "Admin");

    await SeedUserAsync(
        userManager,
        userProfileService,
        configuration["Seed:UserEmail"],
        configuration["Seed:UserPassword"],
        configuration["Seed:UserName"] ?? "User",
        "User");
}

static async Task SeedUserAsync(
    UserManager<AppIdentityUser> userManager,
    IUserProfileService userProfileService,
    string? email,
    string? password,
    string name,
    string role)
{
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return;

    var identityUser = await userManager.FindByEmailAsync(email);
    if (identityUser == null)
    {
        identityUser = new AppIdentityUser
        {
            UserName = email,
            Email = email
        };

        var createResult = await userManager.CreateAsync(identityUser, password);
        if (!createResult.Succeeded)
            return;
    }

    if (!await userManager.IsInRoleAsync(identityUser, role))
        await userManager.AddToRoleAsync(identityUser, role);

    var domainUser = await userProfileService.GetByIdentityUserIdAsync(identityUser.Id);
    if (domainUser == null)
    {
        await userProfileService.CreateProfileAsync(ApiPetFoundation.Domain.Entities.User.Create(identityUser.Id, name));
    }
}
