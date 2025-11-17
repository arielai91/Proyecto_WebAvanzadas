using Microsoft.Extensions.DependencyInjection;
using ApiPetFoundation.Application.Services;

namespace ApiPetFoundation.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            services.AddScoped<PetService>();
            services.AddScoped<PetImageService>();
            services.AddScoped<AdoptionRequestService>();
            services.AddScoped<NotificationService>();

            return services;
        }
    }
}
