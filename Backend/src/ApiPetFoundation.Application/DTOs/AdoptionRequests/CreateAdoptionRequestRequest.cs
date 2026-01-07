namespace ApiPetFoundation.Application.DTOs.AdoptionRequests;

public class CreateAdoptionRequestRequest
{
    public int PetId { get; init; }
    public string Message { get; init; } = string.Empty;
}

