using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class AdoptionRequestService
    {
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;

        public AdoptionRequestService(IAdoptionRequestRepository adoptionRequestRepository)
        {
            _adoptionRequestRepository = adoptionRequestRepository;
        }

        public async Task<IEnumerable<AdoptionRequest>> GetAllAdoptionRequestsAsync()
        {
            return await _adoptionRequestRepository.GetAllAsync();
        }

        public async Task<AdoptionRequest?> GetAdoptionRequestByIdAsync(int id)
        {
            return await _adoptionRequestRepository.GetByIdAsync(id);
        }

        public async Task AddAdoptionRequestAsync(AdoptionRequest request)
        {
            await _adoptionRequestRepository.AddAsync(request);
        }

        public async Task UpdateAdoptionRequestAsync(AdoptionRequest request)
        {
            await _adoptionRequestRepository.UpdateAsync(request);
        }

        public async Task DeleteAdoptionRequestAsync(AdoptionRequest request)
        {
            await _adoptionRequestRepository.DeleteAsync(request);
        }
    }
}
