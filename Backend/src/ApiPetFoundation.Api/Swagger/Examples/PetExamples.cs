using ApiPetFoundation.Application.DTOs.Pets;
using Swashbuckle.AspNetCore.Filters;

namespace ApiPetFoundation.Api.Swagger.Examples;

public class CreatePetRequestExample : IExamplesProvider<CreatePetRequest>
{
    public CreatePetRequest GetExamples()
    {
        return new CreatePetRequest
        {
            Name = "Serafin",
            Species = "Perro",
            Breed = "Schnauzer",
            Age = 3,
            Sex = "Macho",
            Size = "Mediano",
            Description = "Perrito jugueton y amigable."
        };
    }
}

public class UpdatePetRequestExample : IExamplesProvider<UpdatePetRequest>
{
    public UpdatePetRequest GetExamples()
    {
        return new UpdatePetRequest
        {
            Name = "Serafin",
            Species = "Perro",
            Breed = "Schnauzer",
            Age = 4,
            Sex = "Macho",
            Size = "Mediano",
            Description = "Perrito entrenado y activo."
        };
    }
}

public class PatchPetRequestExample : IExamplesProvider<PatchPetRequest>
{
    public PatchPetRequest GetExamples()
    {
        return new PatchPetRequest
        {
            Name = "Serafin",
            Description = "Perrito tranquilo y sociable."
        };
    }
}

public class PetResponseExample : IExamplesProvider<PetResponse>
{
    public PetResponse GetExamples()
    {
        return new PetResponse
        {
            Id = 1,
            Name = "Serafin",
            Species = "Perro",
            Breed = "Schnauzer",
            Age = 3,
            Sex = "Macho",
            Size = "Mediano",
            Description = "Perrito jugueton y amigable.",
            Status = "Available",
            CoverImageUrl = "https://.../pets/1/cover.jpg"
        };
    }
}
