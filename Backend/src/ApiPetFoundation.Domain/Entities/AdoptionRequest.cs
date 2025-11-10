namespace ApiPetFoundation.Domain.Entities
{
    public class AdoptionRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }       // FK → User (quien solicita)
        public int PetId { get; set; }        // FK → Pet
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DecisionAt { get; set; }
        public int? DecisionById { get; set; } // FK → User (admin)

        // 🔗 Relaciones
        public User? User { get; set; }
        public Pet? Pet { get; set; }
        public User? DecisionBy { get; set; }
    }
}
