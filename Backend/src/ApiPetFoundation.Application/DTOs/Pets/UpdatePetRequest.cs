namespace ApiPetFoundation.Application.DTOs.Pets
{
    public class UpdatePetRequest
    {
        public required string Name { get; init; }
        public required string Species { get; init; }
        public required string Breed { get; init; }
        public required int Age { get; init; }
        public required string Sex { get; init; }
        public required string Size { get; init; }
        public required string Description { get; init; }
        public required string Status { get; init; }
    }
}

