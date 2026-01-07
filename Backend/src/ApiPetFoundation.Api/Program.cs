using ApiPetFoundation.Application;
using ApiPetFoundation.Infrastructure;
using FluentValidation;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using ApiPetFoundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:5001");


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
        policy.WithOrigins("http://localhost:4200")
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

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


builder.Services.AddValidatorsFromAssembly(typeof(CreatePetRequest).Assembly);

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

app.UseHttpsRedirection();

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
