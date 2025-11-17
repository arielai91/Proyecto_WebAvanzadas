namespace ApiPetFoundation.Application.DTOs.Pets
{
    public class PetResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Species { get; set; }
        public string? Breed { get; set; }
        public required int Age { get; set; }
        public required string Sex { get; set; }
        public required string Size { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
        public string? CoverImageUrl { get; set; }  // Más adelante con Supabase
    }
}
