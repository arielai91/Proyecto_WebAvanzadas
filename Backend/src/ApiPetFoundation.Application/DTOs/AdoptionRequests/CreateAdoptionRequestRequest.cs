namespace ApiPetFoundation.Application.DTOs.AdoptionRequests;

/// <summary>Datos para crear una solicitud de adopcion. Ej: petId y mensaje.</summary>
public class CreateAdoptionRequestRequest
{
    /// <summary>Id de la mascota a adoptar. Ej: 1</summary>
    public int PetId { get; init; }
    /// <summary>Mensaje opcional para el administrador. Ej: Me encanta esta mascota.</summary>
    public string Message { get; init; } = string.Empty;
}
