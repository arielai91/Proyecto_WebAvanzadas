using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.DTOs.Common;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Services
{
    public interface IPetService
    {
        Pet CreatePetFromDto(CreatePetRequest dto, int userId);
        void UpdatePetFromDto(Pet pet, UpdatePetRequest dto);
        PetResponse MapToResponse(Pet pet);
        PetResponseDetails MapToDetailsResponse(Pet pet);
        Task<IEnumerable<Pet>> GetAllPetsAsync();
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
        Task<Pet?> GetPetByIdAsync(int id);
        Task AddPetAsync(Pet pet);
        Task UpdatePetAsync(Pet pet);
        Task DeletePetAsync(Pet pet);
    }
}
