using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class PetImageService
    {
        private readonly IPetImageRepository _petImageRepository;

        public PetImageService(IPetImageRepository petImageRepository)
        {
            _petImageRepository = petImageRepository;
        }

        public async Task<IEnumerable<PetImage>> GetAllPetImagesAsync()
        {
            return await _petImageRepository.GetAllAsync();
        }

        public async Task<PetImage?> GetPetImageByIdAsync(int id)
        {
            return await _petImageRepository.GetByIdAsync(id);
        }

        public async Task AddPetImageAsync(PetImage petImage)
        {
            await _petImageRepository.AddAsync(petImage);
        }

        public async Task UpdatePetImageAsync(PetImage petImage)
        {
            await _petImageRepository.UpdateAsync(petImage);
        }

        public async Task DeletePetImageAsync(PetImage petImage)
        {
            await _petImageRepository.DeleteAsync(petImage);
        }
    }
}
