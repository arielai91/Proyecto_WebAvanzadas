using Microsoft.AspNetCore.Identity;

namespace ApiPetFoundation.Infrastructure.Identity
{
    public class AppIdentityUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
