namespace TenantService.Application.DTOs;

public record LoginRequestDto (string Username, string Password);
public record RefreshRequestDto (string AccessToken); //, string RefreshToken);
public record LogoutRequestDto (string RefreshToken);

public record TokenPairDto (string AccessToken, string RefreshToken);