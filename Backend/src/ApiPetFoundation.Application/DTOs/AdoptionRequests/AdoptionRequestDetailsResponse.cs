namespace ApiPetFoundation.Application.DTOs.AdoptionRequests
{
    public class AdoptionRequestDetailsResponse
    {
        public int Id { get; init; }
        public int PetId { get; init; }
        public string PetName { get; init; } = string.Empty;
        public int UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? DecisionAt { get; init; }
        public int? DecisionById { get; init; }
    }
}

