namespace ApiPetFoundation.Domain.Entities
{
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string? Breed { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; } = string.Empty; // "Macho" / "Hembra"
        public string Size { get; set; } = string.Empty; // "Pequeño", "Mediano", "Grande"
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public int CreatedById { get; set; } // FK → User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Relaciones
        public User? CreatedBy { get; set; }
        public ICollection<PetImage>? Images { get; set; }
        public ICollection<AdoptionRequest>? AdoptionRequests { get; set; }
    }
}
