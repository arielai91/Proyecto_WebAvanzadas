namespace ApiPetFoundation.Application.DTOs.Auth
{
    /// <summary>Credenciales para iniciar sesion. Ej: correo y clave.</summary>
    public class LoginRequest
    {
        /// <summary>Correo electronico del usuario. Ej: usuario@correo.com</summary>
        public string Email { get; init; } = string.Empty;
        /// <summary>Clave de acceso del usuario. Ej: Clave123</summary>
        public string Password { get; init; } = string.Empty;
    }
}

