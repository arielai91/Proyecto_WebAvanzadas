using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Application.Interfaces.Repositories;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Application.DTOs.Common;
using ApiPetFoundation.Domain.Constants;
using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Services
{
    public class AdoptionRequestService : IAdoptionRequestService
    {
        private readonly IAdoptionRequestRepository _adoptionRequestRepository;
        private readonly IPetRepository _petRepository;
        private readonly INotificationRepository _notificationRepository;

        public AdoptionRequestService(
            IAdoptionRequestRepository adoptionRequestRepository,
            IPetRepository petRepository,
            INotificationRepository notificationRepository)
        {
            _adoptionRequestRepository = adoptionRequestRepository;
            _petRepository = petRepository;
            _notificationRepository = notificationRepository;
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

        public AdoptionRequest CreateFromDto(CreateAdoptionRequestRequest dto, int userId)
        {
            return AdoptionRequest.Create(dto.PetId, userId, dto.Message);
        }

        public async Task<AdoptionRequest> CreateRequestAsync(CreateAdoptionRequestRequest dto, int userId)
        {
            var pet = await _petRepository.GetByIdAsync(dto.PetId);
            if (pet == null)
                throw new ValidationException("Pet not found.");

            if (!pet.IsAvailable())
                throw new ValidationException("Pet is not available for adoption.");

            pet.MarkPending();
            await _petRepository.UpdateAsync(pet);

            var adoptionRequest = AdoptionRequest.Create(dto.PetId, userId, dto.Message);
            await _adoptionRequestRepository.AddAsync(adoptionRequest);

            var adminNotification = Notification.Create(
                pet.CreatedById,
                "AdoptionRequestCreated",
                $"New adoption request for pet {pet.Name}.");
            await _notificationRepository.AddAsync(adminNotification);

            return adoptionRequest;
        }

        public async Task<AdoptionRequest> ApproveAsync(int requestId, int adminUserId)
        {
            var request = await _adoptionRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new ValidationException("Adoption request not found.");

            var pet = await _petRepository.GetByIdAsync(request.PetId);
            if (pet == null)
                throw new ValidationException("Pet not found.");

            request.Approve(adminUserId);
            pet.MarkAdopted();

            await _adoptionRequestRepository.UpdateAsync(request);
            await _petRepository.UpdateAsync(pet);

            var userNotification = Notification.Create(
                request.UserId,
                "AdoptionStatus",
                $"Your adoption request for {pet.Name} was approved.");
            await _notificationRepository.AddAsync(userNotification);

            return request;
        }

        public async Task<AdoptionRequest> RejectAsync(int requestId, int adminUserId)
        {
            var request = await _adoptionRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new ValidationException("Adoption request not found.");

            var pet = await _petRepository.GetByIdAsync(request.PetId);
            if (pet == null)
                throw new ValidationException("Pet not found.");

            request.Reject(adminUserId);
            pet.MarkAvailable();

            await _adoptionRequestRepository.UpdateAsync(request);
            await _petRepository.UpdateAsync(pet);

            var userNotification = Notification.Create(
                request.UserId,
                "AdoptionStatus",
                $"Your adoption request for {pet.Name} was rejected.");
            await _notificationRepository.AddAsync(userNotification);

            return request;
        }

        public async Task<AdoptionRequest> CancelAsync(int requestId, int userId)
        {
            var request = await _adoptionRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new ValidationException("Adoption request not found.");

            if (request.UserId != userId)
                throw new ValidationException("You cannot cancel this request.");

            var pet = await _petRepository.GetByIdAsync(request.PetId);
            if (pet == null)
                throw new ValidationException("Pet not found.");

            request.CancelByRequester();
            pet.MarkAvailable();

            await _adoptionRequestRepository.UpdateAsync(request);
            await _petRepository.UpdateAsync(pet);

            var adminNotification = Notification.Create(
                pet.CreatedById,
                "AdoptionStatus",
                $"Adoption request for {pet.Name} was cancelled by the user.");
            await _notificationRepository.AddAsync(adminNotification);

            return request;
        }

        public async Task<PagedResult<AdoptionRequest>> GetPagedAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById)
        {
            return await _adoptionRequestRepository.GetPagedAsync(
                page,
                pageSize,
                status,
                petId,
                userId,
                decisionById,
                null,
                null);
        }

        public async Task<PagedResult<AdoptionRequest>> GetPagedWithDetailsAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById)
        {
            return await _adoptionRequestRepository.GetPagedAsync(
                page,
                pageSize,
                status,
                petId,
                userId,
                decisionById,
                null,
                null);
        }

        public async Task<PagedResult<AdoptionRequest>> GetPagedWithDetailsAsync(
            int page,
            int pageSize,
            string? status,
            int? petId,
            int? userId,
            int? decisionById,
            DateTime? createdFrom,
            DateTime? createdTo)
        {
            return await _adoptionRequestRepository.GetPagedAsync(
                page,
                pageSize,
                status,
                petId,
                userId,
                decisionById,
                createdFrom,
                createdTo);
        }

        public AdoptionRequestResponse MapToResponse(AdoptionRequest request)
        {
            return new AdoptionRequestResponse
            {
                Id = request.Id,
                PetId = request.PetId,
                UserId = request.UserId,
                Message = request.Message,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            };
        }

        public AdoptionRequestDetailsResponse MapToDetailsResponse(AdoptionRequest request)
        {
            return new AdoptionRequestDetailsResponse
            {
                Id = request.Id,
                PetId = request.PetId,
                PetName = request.Pet?.Name ?? string.Empty,
                UserId = request.UserId,
                UserName = request.User?.Name ?? string.Empty,
                Message = request.Message,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                DecisionAt = request.DecisionAt,
                DecisionById = request.DecisionById
            };
        }
    }
}
