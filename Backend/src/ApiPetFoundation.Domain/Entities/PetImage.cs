namespace ApiPetFoundation.Domain.Entities
{
    public class PetImage
    {
        public int Id { get; set; }
        public int PetId { get; set; }       // FK → Pet
        public string Url { get; set; } = string.Empty;
        public bool IsCover { get; set; } = true;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Relaciones
        public Pet? Pet { get; set; }
    }
}
