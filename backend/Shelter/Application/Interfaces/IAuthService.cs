using Application.Dtos.Auth_Identity;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken ct = default);
        Task<ApplicationUser?> GetUserByIdAsync(string id, CancellationToken ct = default);
    }
}
