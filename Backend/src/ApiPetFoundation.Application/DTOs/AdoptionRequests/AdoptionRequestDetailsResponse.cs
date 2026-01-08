namespace ApiPetFoundation.Application.DTOs.AdoptionRequests
{
    /// <summary>Datos detallados de una solicitud de adopcion.</summary>
    public class AdoptionRequestDetailsResponse
    {
        /// <summary>Id de la solicitud.</summary>
        public int Id { get; init; }
        /// <summary>Id de la mascota solicitada.</summary>
        public int PetId { get; init; }
        /// <summary>Nombre de la mascota.</summary>
        public string PetName { get; init; } = string.Empty;
        /// <summary>Id del usuario solicitante.</summary>
        public int UserId { get; init; }
        /// <summary>Nombre del usuario solicitante.</summary>
        public string UserName { get; init; } = string.Empty;
        /// <summary>Mensaje enviado con la solicitud.</summary>
        public string Message { get; init; } = string.Empty;
        /// <summary>Estado actual de la solicitud.</summary>
        public string Status { get; init; } = string.Empty;
        /// <summary>Fecha de creacion.</summary>
        public DateTime CreatedAt { get; init; }
        /// <summary>Fecha de decision (si aplica).</summary>
        public DateTime? DecisionAt { get; init; }
        /// <summary>Id del usuario que tomo la decision.</summary>
        public int? DecisionById { get; init; }
    }
}
