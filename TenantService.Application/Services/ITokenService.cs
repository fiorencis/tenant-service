using System.Security.Claims;
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;

namespace TenantService.Application.Services;

public interface ITokenService : IApplicationService
{
    string GenerateAccessToken(string username, string role);
    RefreshToken GenerateRefreshToken(string username);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<TokenPairDto> CreateTokenPair(string username, string role, CancellationToken cancellationToken = default);

    Task<TokenPairDto> RefreshTokenPairAsync(string accessToken, string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}
