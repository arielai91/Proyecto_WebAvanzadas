using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Infrastructure.Repositories;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiPetFoundation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar el DbContext con PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Registrar servicios de autenticación

            services.AddIdentity<AppIdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 3. JWT Authentication
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });


            // 4. Registrar AuthService
            services.AddScoped<IAuthService, AuthService>();

            // Registrar repositorios
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPetImageRepository, PetImageRepository>();
            services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            return services;
        }
    }
}
