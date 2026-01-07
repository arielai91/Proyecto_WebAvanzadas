namespace ApiPetFoundation.Application.DTOs.AdoptionRequests;

public class AdoptionRequestResponse
{
    public int Id { get; init; }
    public int PetId { get; init; }
    public int UserId { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

