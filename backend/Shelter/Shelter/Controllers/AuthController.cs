using Application.Dtos.Auth_Identity;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Shelter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken ct)
        {
            try
            {
                var res = await _authService.RegisterAsync(registerDto, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken ct)
        {
            try
            {
                var res = await _authService.LoginAsync(loginDto, ct);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            // odczytanie ID zalogowanego użytkownika z JWT tokena
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new AuthResponseDto
            {
                Token = "", // /me nie generuje nowego tokena
                UserId = user.Id,
                Email = user.Email!,
                Roles = roles,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }
    }
}
