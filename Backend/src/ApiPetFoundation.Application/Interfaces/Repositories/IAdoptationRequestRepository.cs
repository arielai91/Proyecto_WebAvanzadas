using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Common;

namespace ApiPetFoundation.Application.Interfaces.Repositories
{
    public interface IAdoptionRequestRepository : IRepository<AdoptionRequest>
    {
        Task<PagedResult<AdoptionRequest>> GetPagedAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById,
            DateTime? createdFrom,
            DateTime? createdTo);
    }
}
