namespace ApiPetFoundation.Application.DTOs.Pets
{
    public class CreatePetRequest
    {
        public required string Name { get; set; }
        public required string Species { get; set; }
        public required string Breed { get; set; }
        public required int Age { get; set; }
        public required string Sex { get; set; }
        public required string Size { get; set; }
        public required string Description { get; set; }
    }
}
