using ApiPetFoundation.Application.DTOs.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ApiPetFoundation.Api.Swagger.Examples;

public class RegisterRequestExample : IExamplesProvider<RegisterRequest>
{
    public RegisterRequest GetExamples()
    {
        return new RegisterRequest
        {
            Email = "usuario@correo.com",
            Name = "Ariel",
            Password = "Clave123"
        };
    }
}

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "usuario@correo.com",
            Password = "Clave123"
        };
    }
}

public class AuthResponseExample : IExamplesProvider<AuthResponse>
{
    public AuthResponse GetExamples()
    {
        return new AuthResponse
        {
            Token = "eyJhbGciOi...",
            Email = "usuario@correo.com",
            Name = "Ariel",
            Roles = new List<string> { "User" }
        };
    }
}
