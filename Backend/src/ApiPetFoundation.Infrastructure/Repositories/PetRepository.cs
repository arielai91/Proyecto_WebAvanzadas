using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.DTOs.Common;
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
            return await _context.Pets.Include(p => p.Images).ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(int id)
        {
            return await _context.Pets.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
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

        public async Task<PagedResult<Pet>> GetPagedAsync(
            int page,
            int pageSize,
            string? status,
            string? species,
            string? size,
            string? sex,
            int? createdById,
            int? minAge,
            int? maxAge,
            string? search,
            DateTime? createdFrom,
            DateTime? createdTo)
        {
            var query = _context.Pets
                .Include(p => p.Images)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrWhiteSpace(species))
                query = query.Where(p => p.Species.ToLower() == species.ToLower());

            if (!string.IsNullOrWhiteSpace(size))
                query = query.Where(p => p.Size.ToLower() == size.ToLower());

            if (!string.IsNullOrWhiteSpace(sex))
                query = query.Where(p => p.Sex.ToLower() == sex.ToLower());

            if (createdById.HasValue)
                query = query.Where(p => p.CreatedById == createdById.Value);

            if (minAge.HasValue)
                query = query.Where(p => p.Age >= minAge.Value);

            if (maxAge.HasValue)
                query = query.Where(p => p.Age <= maxAge.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchValue = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchValue) ||
                    (p.Breed != null && p.Breed.ToLower().Contains(searchValue)));
            }

            if (createdFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(p => p.CreatedAt <= createdTo.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Pet>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
