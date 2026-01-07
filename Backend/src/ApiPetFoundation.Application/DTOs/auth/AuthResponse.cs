namespace ApiPetFoundation.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public List<string>? Roles { get; init; }

    }
}

