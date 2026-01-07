namespace ApiPetFoundation.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string IdentityUserId { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;
        public string? PhotoUrl { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // 🔗 Relaciones
        public ICollection<Pet>? PetsCreated { get; private set; }   // Admin publica mascotas
        public ICollection<AdoptionRequest>? AdoptionRequests { get; private set; } // Solicitudes enviadas
        public ICollection<Notification>? Notifications { get; private set; } // Notificaciones del usuario

        private User() { }

        public static User Create(string identityUserId, string name, string? photoUrl = null)
        {
            return new User
            {
                IdentityUserId = identityUserId,
                Name = name,
                PhotoUrl = photoUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateProfile(string name, string? photoUrl)
        {
            Name = name;
            PhotoUrl = photoUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
