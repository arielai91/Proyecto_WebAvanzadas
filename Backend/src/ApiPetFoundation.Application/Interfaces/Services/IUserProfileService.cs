using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task CreateProfileAsync(User user);
    Task<User?> GetByIdentityUserIdAsync(string identityUserId);
}
