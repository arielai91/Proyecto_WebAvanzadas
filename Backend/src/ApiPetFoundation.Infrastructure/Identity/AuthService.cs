using ApiPetFoundation.Application.DTOs.Auth;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Persistence.Contexts;
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
        private readonly AppDbContext _dbContext;

        public AuthService(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IConfiguration config,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. Crear usuario en Identity
            var identityUser = new AppIdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.Name
            };

            var result = await _userManager.CreateAsync(identityUser, request.Password);

            if (!result.Succeeded)
                throw new ValidationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

            // 2. Asignar rol usuario por defecto
            await _userManager.AddToRoleAsync(identityUser, "User");

            // 2. Crear user del dominio (perfil)
            var userDomain = new User
            {
                IdentityUserId = identityUser.Id,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.UsersDomain.Add(userDomain);
            await _dbContext.SaveChangesAsync();

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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtener roles de Identity
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim("name", user.DisplayName ?? "")
            };

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
                Name = user.DisplayName ?? "",
                Roles = roles.ToList()
            };
        }

    }
}
