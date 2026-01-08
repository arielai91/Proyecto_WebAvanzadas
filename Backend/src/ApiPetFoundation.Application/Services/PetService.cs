using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using ApiPetFoundation.Application.DTOs.Common;
namespace ApiPetFoundation.Application.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        // Convert DTO -> Entity
        public Pet CreatePetFromDto(CreatePetRequest dto, int userId)
        {
            return Pet.Create(
                dto.Name,
                dto.Species,
                dto.Breed,
                dto.Age,
                dto.Sex,
                dto.Size,
                dto.Description,
                userId);
        }

        // Update entity from DTO
        public void UpdatePetFromDto(Pet pet, UpdatePetRequest dto)
        {
            pet.UpdateDetails(
                dto.Name,
                dto.Species,
                dto.Breed,
                dto.Age,
                dto.Sex,
                dto.Size,
                dto.Description);
        }

        public void PatchPetFromDto(Pet pet, PatchPetRequest dto)
        {
            pet.PatchDetails(
                dto.Name,
                dto.Species,
                dto.Breed,
                dto.Age,
                dto.Sex,
                dto.Size,
                dto.Description);
        }

        // Convert Entity -> Response DTO (para Swagger)
        public PetResponse MapToResponse(Pet pet)
        {
            var coverImageUrl = pet.Images?.FirstOrDefault(i => i.IsCover)?.Url;
            return new PetResponse
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Age = pet.Age,
                Sex = pet.Sex,
                Size = pet.Size,
                Description = pet.Description,
                Status = pet.Status,
                CoverImageUrl = coverImageUrl
            };
        }

        public PetResponseDetails MapToDetailsResponse(Pet pet)
        {
            var coverImageUrl = pet.Images?.FirstOrDefault(i => i.IsCover)?.Url;
            return new PetResponseDetails
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Age = pet.Age,
                Sex = pet.Sex,
                Size = pet.Size,
                Description = pet.Description,
                Status = pet.Status,
                CreatedById = pet.CreatedById,
                CoverImageUrl = coverImageUrl ?? string.Empty,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt
            };
        }

        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _petRepository.GetAllAsync();
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
            return await _petRepository.GetPagedAsync(
                page,
                pageSize,
                status,
                species,
                size,
                sex,
                createdById,
                minAge,
                maxAge,
                search,
                createdFrom,
                createdTo);
        }

        public async Task<Pet?> GetPetByIdAsync(int id)
        {
            return await _petRepository.GetByIdAsync(id);
        }

        public async Task AddPetAsync(Pet pet)
        {
            await _petRepository.AddAsync(pet);
        }

        public async Task UpdatePetAsync(Pet pet)
        {
            await _petRepository.UpdateAsync(pet);
        }

        public async Task DeletePetAsync(Pet pet)
        {
            await _petRepository.DeleteAsync(pet);
        }
    }
}
