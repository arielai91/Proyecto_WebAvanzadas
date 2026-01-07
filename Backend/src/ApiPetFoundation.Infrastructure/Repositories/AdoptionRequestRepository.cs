using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.DTOs.Common;
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

        public async Task<PagedResult<AdoptionRequest>> GetPagedAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById,
            DateTime? createdFrom,
            DateTime? createdTo)
        {
            var query = _context.AdoptionRequests
                .Include(r => r.Pet)
                .Include(r => r.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status.ToLower() == status.ToLower());

            if (petId.HasValue)
                query = query.Where(r => r.PetId == petId.Value);

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (decisionById.HasValue)
                query = query.Where(r => r.DecisionById == decisionById.Value);

            if (createdFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(r => r.CreatedAt <= createdTo.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AdoptionRequest>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
