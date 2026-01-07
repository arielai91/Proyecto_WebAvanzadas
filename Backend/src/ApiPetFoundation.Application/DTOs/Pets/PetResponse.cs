namespace ApiPetFoundation.Application.DTOs.Pets
{
    public class PetResponse
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required string Species { get; init; }
        public string? Breed { get; init; }
        public required int Age { get; init; }
        public required string Sex { get; init; }
        public required string Size { get; init; }
        public required string Description { get; init; }
        public required string Status { get; init; }
        public string? CoverImageUrl { get; init; }  // Más adelante con Supabase
    }
}

