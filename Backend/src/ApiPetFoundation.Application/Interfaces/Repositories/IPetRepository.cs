using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Common;

namespace ApiPetFoundation.Application.Interfaces.Repositories
{
    public interface IPetRepository : IRepository<Pet>
    {
        Task<PagedResult<Pet>> GetPagedAsync(
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
            DateTime? createdTo);
    }
}
