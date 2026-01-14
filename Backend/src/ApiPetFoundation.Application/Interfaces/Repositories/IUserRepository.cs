using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdentityUserIdAsync(string identityUserId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    }
}
