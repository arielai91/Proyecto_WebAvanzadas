using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using Swashbuckle.AspNetCore.Filters;

namespace ApiPetFoundation.Api.Swagger.Examples;

public class CreateAdoptionRequestExample : IExamplesProvider<CreateAdoptionRequestRequest>
{
    public CreateAdoptionRequestRequest GetExamples()
    {
        return new CreateAdoptionRequestRequest
        {
            PetId = 1,
            Message = "Me gustaria adoptar esta mascota."
        };
    }
}

public class AdoptionRequestResponseExample : IExamplesProvider<AdoptionRequestResponse>
{
    public AdoptionRequestResponse GetExamples()
    {
        return new AdoptionRequestResponse
        {
            Id = 10,
            PetId = 1,
            UserId = 2,
            Message = "Me gustaria adoptar esta mascota.",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }
}
