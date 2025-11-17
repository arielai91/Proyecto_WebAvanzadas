using ApiPetFoundation.Application.DTOs.Auth;

namespace ApiPetFoundation.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
