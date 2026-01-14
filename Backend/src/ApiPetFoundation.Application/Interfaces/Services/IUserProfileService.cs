using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task CreateProfileAsync(User user);
    Task<User?> GetByIdentityUserIdAsync(string identityUserId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
}
