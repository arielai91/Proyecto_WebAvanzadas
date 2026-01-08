using ApiPetFoundation.Application.DTOs.Auth;
using ApiPetFoundation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using ApiPetFoundation.Api.Swagger.Examples;
using System.Security.Claims;

namespace ApiPetFoundation.Api.Controllers
{
    /// <summary>Autenticacion y perfil del usuario.</summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        /// <summary>Registro de usuario (Publico).</summary>
        /// <param name="request">Datos de registro.</param>
        /// <returns>Token y datos del usuario.</returns>
        [HttpPost("register")]
        [SwaggerRequestExample(typeof(RegisterRequest), typeof(RegisterRequestExample))]
        [SwaggerResponseExample(200, typeof(AuthResponseExample))]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST: api/auth/login
        /// <summary>Login de usuario (Publico).</summary>
        /// <param name="request">Credenciales de acceso.</param>
        /// <returns>Token y datos del usuario.</returns>
        [HttpPost("login")]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [SwaggerResponseExample(200, typeof(AuthResponseExample))]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // GET: api/auth/me
        /// <summary>Devuelve el usuario autenticado (User, Admin).</summary>
        /// <returns>Datos basicos del usuario autenticado.</returns>
        [HttpGet("me")]
        [Authorize()]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Name = User.FindFirst("name")?.Value,
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value)
            });
        }
    }
}
