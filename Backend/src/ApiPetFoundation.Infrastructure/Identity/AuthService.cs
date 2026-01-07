using ApiPetFoundation.Application.DTOs.Auth;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiPetFoundation.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IUserProfileService _userProfileService;

        public AuthService(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IConfiguration config,
            IUserProfileService userProfileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _userProfileService = userProfileService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. Crear usuario en Identity
            var identityUser = new AppIdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(identityUser, request.Password);

            if (!result.Succeeded)
                throw new ValidationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

            // 2. Asignar rol usuario por defecto
            await _userManager.AddToRoleAsync(identityUser, "User");

            // 2. Crear user del dominio (perfil)
            var userDomain = User.Create(identityUser.Id, request.Name);

            await _userProfileService.CreateProfileAsync(userDomain);

            // 3. Generar token
            return await GenerateTokenResponse(identityUser);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Email);

            if (identityUser == null)
                throw new AuthException("Invalid credentials.");

            var result = await _signInManager.CheckPasswordSignInAsync(identityUser, request.Password, false);

            if (!result.Succeeded)
                throw new AuthException("Invalid credentials.");

            return await GenerateTokenResponse(identityUser);
        }

        private async Task<AuthResponse> GenerateTokenResponse(AppIdentityUser user)
        {
            var domainUser = await _userProfileService.GetByIdentityUserIdAsync(user.Id);
            var displayName = domainUser?.Name ?? user.UserName ?? user.Email ?? string.Empty;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtener roles de Identity
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim("name", displayName)
            };

            if (domainUser != null)
            {
                claims.Add(new Claim("domain_user_id", domainUser.Id.ToString()));
            }

            // Agregar roles al JWT
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email!,
                Name = displayName,
                Roles = roles.ToList()
            };
        }

    }
}
