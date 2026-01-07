using ApiPetFoundation.Domain.Constants;

namespace ApiPetFoundation.Domain.Entities
{
    public class AdoptionRequest
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }       // FK  User (quien solicita)
        public int PetId { get; private set; }        // FK  Pet
        public string Message { get; private set; } = string.Empty;
        public string Status { get; private set; } = AdoptionRequestStatuses.Pending;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? DecisionAt { get; private set; }
        public int? DecisionById { get; private set; } // FK  User (admin)

        // ?? Relaciones
        public User? User { get; private set; }
        public Pet? Pet { get; private set; }
        public User? DecisionBy { get; private set; }

        private AdoptionRequest() { }

        public static AdoptionRequest Create(int petId, int userId, string message)
        {
            return new AdoptionRequest
            {
                PetId = petId,
                UserId = userId,
                Message = message,
                Status = AdoptionRequestStatuses.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Approve(int adminUserId)
        {
            if (Status != AdoptionRequestStatuses.Pending)
                throw new InvalidOperationException("Only pending requests can be approved.");

            Status = AdoptionRequestStatuses.Approved;
            DecisionAt = DateTime.UtcNow;
            DecisionById = adminUserId;
        }

        public void Reject(int adminUserId)
        {
            if (Status != AdoptionRequestStatuses.Pending)
                throw new InvalidOperationException("Only pending requests can be rejected.");

            Status = AdoptionRequestStatuses.Rejected;
            DecisionAt = DateTime.UtcNow;
            DecisionById = adminUserId;
        }

        public void CancelByRequester()
        {
            if (Status != AdoptionRequestStatuses.Pending)
                throw new InvalidOperationException("Only pending requests can be cancelled.");

            Status = AdoptionRequestStatuses.Cancelled;
            DecisionAt = DateTime.UtcNow;
        }
    }
}
