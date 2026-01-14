namespace ApiPetFoundation.Application.DTOs.Auth
{
    /// <summary>Respuesta con token y datos del usuario.</summary>
    public class AuthResponse
    {
        /// <summary>JWT para autenticar solicitudes.</summary>
        public string Token { get; init; } = string.Empty;
        /// <summary>ID del usuario en el dominio.</summary>
        public int UserId { get; init; }
        /// <summary>Correo electronico del usuario.</summary>
        public string Email { get; init; } = string.Empty;
        /// <summary>Nombre visible del usuario.</summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>Roles asignados al usuario.</summary>
        public List<string>? Roles { get; init; }

    }
}

