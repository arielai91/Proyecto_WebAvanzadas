namespace ApiPetFoundation.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }            // FK → Users
        public string Type { get; set; } = string.Empty;  // Ej: "AdoptionStatus", "NewPet"
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
