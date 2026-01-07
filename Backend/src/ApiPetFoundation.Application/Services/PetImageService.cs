using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class PetImageService
    {
        private readonly IPetImageRepository _petImageRepository;
        private readonly IPetRepository _petRepository;

        public PetImageService(IPetImageRepository petImageRepository, IPetRepository petRepository)
        {
            _petImageRepository = petImageRepository;
            _petRepository = petRepository;
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

        public async Task<PetImage> SetCoverImageAsync(int petId, string imageUrl)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null)
                throw new InvalidOperationException("Pet not found.");

            var existingImages = await _petImageRepository.GetByPetIdAsync(petId);
            foreach (var image in existingImages.Where(i => i.IsCover))
            {
                image.SetCover(false);
                await _petImageRepository.UpdateAsync(image);
            }

            var newImage = PetImage.Create(petId, imageUrl, true);

            await _petImageRepository.AddAsync(newImage);
            return newImage;
        }
    }
}
