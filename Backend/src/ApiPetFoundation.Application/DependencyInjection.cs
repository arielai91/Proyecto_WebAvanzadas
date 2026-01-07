using Microsoft.Extensions.DependencyInjection;
using ApiPetFoundation.Application.Services;
using ApiPetFoundation.Application.Interfaces.Services;

namespace ApiPetFoundation.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<PetImageService>();
            services.AddScoped<IAdoptionRequestService, AdoptionRequestService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
