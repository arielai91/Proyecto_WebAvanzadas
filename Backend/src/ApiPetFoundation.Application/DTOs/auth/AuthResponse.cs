namespace ApiPetFoundation.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }

    }
}
