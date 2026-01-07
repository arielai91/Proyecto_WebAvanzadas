using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.UsersDomain.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.UsersDomain.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _context.UsersDomain.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }

        public async Task AddAsync(User entity)
        {
            await _context.UsersDomain.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            _context.UsersDomain.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User entity)
        {
            _context.UsersDomain.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
