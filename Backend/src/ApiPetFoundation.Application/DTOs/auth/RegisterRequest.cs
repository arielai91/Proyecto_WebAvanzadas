namespace ApiPetFoundation.Application.DTOs.Auth
{
    /// <summary>Datos para registrar un usuario. Ej: correo, nombre y clave.</summary>
    public class RegisterRequest
    {
        /// <summary>Correo electronico del usuario. Ej: usuario@correo.com</summary>
        public string Email { get; init; } = string.Empty;
        /// <summary>Nombre visible del usuario. Ej: Ariel</summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>Clave de acceso del usuario. Ej: Clave123</summary>
        public string Password { get; init; } = string.Empty;
    }
}

