using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Repositories
{
    public class PetImageRepository : IPetImageRepository
    {
        private readonly AppDbContext _context;

        public PetImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PetImage>> GetAllAsync()
        {
            return await _context.PetImages.ToListAsync();
        }

        public async Task<PetImage?> GetByIdAsync(int id)
        {
            return await _context.PetImages.FindAsync(id);
        }

        public async Task AddAsync(PetImage entity)
        {
            _context.PetImages.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PetImage entity)
        {
            _context.PetImages.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PetImage entity)
        {
            _context.PetImages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}