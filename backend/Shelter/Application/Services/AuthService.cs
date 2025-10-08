using Application.Dtos.Auth_Identity;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        // zarzadzanie uzytkownikami (rejestracja, hasla, role)
        private readonly UserManager<ApplicationUser> _userManager;
        // logowanie, weryfikacja hasel, 2FA
        private readonly SignInManager<ApplicationUser> _signInManager;
        // dostep do zmiennych srodowiskowych
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = configuration;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id, CancellationToken ct = default)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) 
                throw new KeyNotFoundException("Nieprawidłowy e-mail lub hasło.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!check.Succeeded)
                throw new KeyNotFoundException("Nieprawidłowy e-mail lub hasło.");

            var token = await GenerateJwtAsync(user);
            return await BuildResponseAsync(user, token);
        }
         
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default)
        {
            var exists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (exists != null)
                throw new InvalidOperationException("Użytkownik o tym e-mailu już istnieje.");

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                RegisteredAt = DateTime.UtcNow
            };

            var create = await _userManager.CreateAsync(user,registerDto.Password);
            if(!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Rejestracja nieudana: {errors}");
            }

            //default role
            await _userManager.AddToRoleAsync(user, "Client");

            var token = await GenerateJwtAsync(user);
            return await BuildResponseAsync(user, token);
        }

        private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user, string token)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles.ToList(),
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        private async Task<string> GenerateJwtAsync(ApplicationUser user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = jwt["Key"] ?? throw new InvalidOperationException("Brak Jwt:Key w konfiguracji");
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];
            var expiresMinutes = int.TryParse(jwt["ExpiresMinutes"], out var m) ? m : 240;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("firstName", user.FirstName ?? string.Empty),
                new Claim("lastName",  user.LastName  ?? string.Empty),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
