namespace ApiPetFoundation.Application.DTOs.AdoptionRequests;

/// <summary>Datos basicos de una solicitud de adopcion.</summary>
public class AdoptionRequestResponse
{
    /// <summary>Id de la solicitud.</summary>
    public int Id { get; init; }
    /// <summary>Id de la mascota solicitada.</summary>
    public int PetId { get; init; }
    /// <summary>Id del usuario solicitante.</summary>
    public int UserId { get; init; }
    /// <summary>Mensaje enviado con la solicitud.</summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>Estado actual de la solicitud.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Fecha de creacion.</summary>
    public DateTime CreatedAt { get; init; }
}
