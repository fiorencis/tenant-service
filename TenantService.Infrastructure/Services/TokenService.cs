using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TenantService.Application.DTOs;
using TenantService.Application.Repositories;
using TenantService.Domain.Entities;
using TenantService.Infrastructure;

namespace TenantService.Application.Services;

public class TokenService : ApplicationService, ITokenService
{
    private readonly IRepository<RefreshToken> _tokenRepository;
	private readonly IUnitOfWork _unitOfWork;

   
    private readonly TenantDbContext _tenantDbContext;

    public TokenService(IRepository<RefreshToken> tokenRepository, IUnitOfWork unitOfWork, 
        IConfiguration config, ILogger<TokenService> logger) : base(config, logger)
    {
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
    } 

    // Generates a JWT access token for the specified username and role
    public string GenerateAccessToken(string username, string role)
    {
        var jwt = _config.GetSection("Jwt");

        var secret = Environment.GetEnvironmentVariable("TOKEN_KEY") 
        ?? _config.GetValue<string>(jwt["Key"]) 
        ?? "supersecretkey1234567890!@#$%^&*()@@_$QuLoW%qwerty&potrimao99@###][";

        var expiryMinutes = Environment.GetEnvironmentVariable("TOKEN_EXPIRY_MINUTES") ?? 
        jwt["ExpiryMinutes"] ?? "60"; // Default to 60 minutes if not set

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(expiryMinutes!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    // Generates a refresh token for the specified username
    public RefreshToken GenerateRefreshToken(string username)
    {
        var jwt = _config.GetSection("Jwt");

        return new RefreshToken
        {
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Username  = username,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(jwt["RefreshExpiryDays"]!)),
            IsRevoked = false
        };
    }

    public async Task<TokenPairDto> CreateTokenPair(string username, string role, CancellationToken cancellationToken = default)
    {
        var accessToken = GenerateAccessToken(username, role);
        var refreshToken = GenerateRefreshToken(username);

        _logger.LogInformation("Creating new token pair for user {Username}", username);

        await _tokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenPairDto(accessToken, refreshToken.Token);
    }

    // Retrieves the claims principal from an expired JWT token
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwt = _config.GetSection("Jwt");

        var secret = Environment.GetEnvironmentVariable("TOKEN_KEY") 
        ?? _config.GetValue<string>(jwt["Key"]) 
        ?? "supersecretkey1234567890!@#$%^&*()@@_$QuLoW%qwerty&potrimao99@###][";

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = jwt["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwt["Audience"],
            ValidateLifetime         = false, // ← ignora la scadenza intenzionalmente
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, validationParams, out var validatedToken);

        // Assicurati che sia effettivamente un JWT con HMAC-SHA256
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            return null;

        return principal;
    }

    public async Task<TokenPairDto> RefreshTokenPairAsync(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);

        if (principal == null)
        {
            throw new SecurityTokenException("Invalid access token");
        }

        var username = principal.Identity?.Name;
        if (username == null)
        {
            throw new SecurityTokenException("Invalid access token");
        }

        _logger.LogInformation($"Attempting to refresh token for user {username} with refresh token {refreshToken}");

        var storedRefreshTokenList = await _tokenRepository.ListAsync(rt => rt.Token == refreshToken && rt.Username == username);
        var storedRefreshToken = storedRefreshTokenList.FirstOrDefault();
        
        if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // Invalidate the old refresh token
        storedRefreshToken.IsRevoked = true;
        _tokenRepository.Update(storedRefreshToken);

        _logger.LogInformation("Refresh token {RefreshToken} for user {Username} has been revoked", storedRefreshToken.Token, username);

        return await CreateTokenPair(username, principal.FindFirst(ClaimTypes.Role)?.Value ?? "User");
    }
}
