namespace ApiPetFoundation.Domain.Entities
{
    public class PetImage
    {
        public int Id { get; private set; }
        public int PetId { get; private set; }       // FK  Pet
        public string Url { get; private set; } = string.Empty;
        public bool IsCover { get; private set; }
        public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

        // ?? Relaciones
        public Pet? Pet { get; private set; }

        private PetImage() { }

        public static PetImage Create(int petId, string url, bool isCover)
        {
            return new PetImage
            {
                PetId = petId,
                Url = url,
                IsCover = isCover,
                UploadedAt = DateTime.UtcNow
            };
        }

        public void SetCover(bool isCover)
        {
            IsCover = isCover;
        }
    }
}
