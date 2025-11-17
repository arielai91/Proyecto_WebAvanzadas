using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Application.DTOs.Pets;
namespace ApiPetFoundation.Application.Services
{
    public class PetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        // Convert DTO -> Entity
        public Pet CreatePetFromDto(CreatePetRequest dto, int userId)
        {
            return new Pet
            {
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Age = dto.Age,
                Sex = dto.Sex,
                Size = dto.Size,
                Description = dto.Description,
                Status = "Available",
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Update entity from DTO
        public void UpdatePetFromDto(Pet pet, UpdatePetRequest dto)
        {
            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.Age = dto.Age;
            pet.Sex = dto.Sex;
            pet.Size = dto.Size;
            pet.Description = dto.Description;
            pet.Status = dto.Status;
            pet.UpdatedAt = DateTime.UtcNow;
        }

        // Convert Entity -> Response DTO (para Swagger)
        public PetResponse MapToResponse(Pet pet)
        {
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
                CoverImageUrl = null // lo agregaremos con Supabase
            };
        }

        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _petRepository.GetAllAsync();
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
