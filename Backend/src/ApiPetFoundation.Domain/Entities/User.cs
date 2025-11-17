namespace ApiPetFoundation.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string IdentityUserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Relaciones
        public ICollection<Pet>? PetsCreated { get; set; }   // Admin publica mascotas
        public ICollection<AdoptionRequest>? AdoptionRequests { get; set; } // Solicitudes enviadas
        public ICollection<Notification>? Notifications { get; set; } // Notificaciones del usuario
    }
}
