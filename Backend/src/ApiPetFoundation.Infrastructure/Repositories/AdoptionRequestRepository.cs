using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ApiPetFoundation.Infrastructure.Repositories
{
    public class AdoptionRequestRepository : IAdoptionRequestRepository
    {
        private readonly AppDbContext _context;

        public AdoptionRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdoptionRequest>> GetAllAsync()
        {
            return await _context.AdoptionRequests.ToListAsync();
        }

        public async Task<AdoptionRequest?> GetByIdAsync(int id)
        {
            return await _context.AdoptionRequests.FindAsync(id);
        }

        public async Task AddAsync(AdoptionRequest entity)
        {
            _context.AdoptionRequests.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AdoptionRequest entity)
        {
            _context.AdoptionRequests.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AdoptionRequest entity)
        {
            _context.AdoptionRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}