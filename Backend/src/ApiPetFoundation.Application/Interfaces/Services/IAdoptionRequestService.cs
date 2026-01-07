using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.DTOs.Common;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Services
{
    public interface IAdoptionRequestService
    {
        Task<IEnumerable<AdoptionRequest>> GetAllAdoptionRequestsAsync();
        Task<AdoptionRequest?> GetAdoptionRequestByIdAsync(int id);
        Task AddAdoptionRequestAsync(AdoptionRequest request);
        Task UpdateAdoptionRequestAsync(AdoptionRequest request);
        Task DeleteAdoptionRequestAsync(AdoptionRequest request);
        AdoptionRequest CreateFromDto(CreateAdoptionRequestRequest dto, int userId);
        Task<AdoptionRequest> CreateRequestAsync(CreateAdoptionRequestRequest dto, int userId);
        Task<AdoptionRequest> ApproveAsync(int requestId, int adminUserId);
        Task<AdoptionRequest> RejectAsync(int requestId, int adminUserId);
        Task<AdoptionRequest> CancelAsync(int requestId, int userId);
        Task<PagedResult<AdoptionRequest>> GetPagedAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById);
        Task<PagedResult<AdoptionRequest>> GetPagedWithDetailsAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById);
        Task<PagedResult<AdoptionRequest>> GetPagedWithDetailsAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById,
            DateTime? createdFrom,
            DateTime? createdTo);
        AdoptionRequestDetailsResponse MapToDetailsResponse(AdoptionRequest request);
        AdoptionRequestResponse MapToResponse(AdoptionRequest request);
    }
}
