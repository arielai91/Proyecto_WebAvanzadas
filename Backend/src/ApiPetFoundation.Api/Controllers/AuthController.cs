using ApiPetFoundation.Application.DTOs.Auth;
using ApiPetFoundation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiPetFoundation.Api.Controllers
{
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
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // GET: api/auth/me
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
