namespace ApiPetFoundation.Application.DTOs.Pets
{
    public class PetResponseDetails
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Species { get; init; } = string.Empty;
        public string? Breed { get; init; }
        public int Age { get; init; }
        public string Sex { get; init; } = string.Empty;
        public string Size { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public int CreatedById { get; init; }
        public string CoverImageUrl { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}

