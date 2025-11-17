using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;

        public PetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pet>> GetAllAsync()
        {
            return await _context.Pets.ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(int id)
        {
            return await _context.Pets.FindAsync(id);
        }

        public async Task AddAsync(Pet entity)
        {
            _context.Pets.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet entity)
        {
            _context.Pets.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Pet entity)
        {
            _context.Pets.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}