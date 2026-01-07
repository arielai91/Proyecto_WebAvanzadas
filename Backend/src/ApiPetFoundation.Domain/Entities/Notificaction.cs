namespace ApiPetFoundation.Domain.Entities
{
    public class Notification
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }            // FK  Users
        public string Type { get; private set; } = string.Empty;  // Ej: "AdoptionStatus", "NewPet"
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public User User { get; private set; } = null!;

        private Notification() { }

        public static Notification Create(int userId, string type, string message)
        {
            return new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
