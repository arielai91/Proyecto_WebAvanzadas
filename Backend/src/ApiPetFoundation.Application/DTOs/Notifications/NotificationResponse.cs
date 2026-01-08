namespace ApiPetFoundation.Application.DTOs.Notifications
{
    /// <summary>Datos de una notificacion.</summary>
    public class NotificationResponse
    {
        /// <summary>Id de la notificacion.</summary>
        public int Id { get; init; }
        /// <summary>Tipo de notificacion.</summary>
        public string Type { get; init; } = string.Empty;
        /// <summary>Mensaje de la notificacion.</summary>
        public string Message { get; init; } = string.Empty;
        /// <summary>Indica si la notificacion ya fue leida.</summary>
        public bool IsRead { get; init; }
        /// <summary>Fecha de creacion.</summary>
        public DateTime CreatedAt { get; init; }
    }
}
