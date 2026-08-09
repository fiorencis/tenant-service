using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantService.API.Controllers;
using TenantService.API.Controllers.Auth;
using TenantService.Application;
using TenantService.Application.DTOs;
using TenantService.Application.Services;

namespace TenantService.API;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController : TenantBaseController
{
    protected readonly IUserService _userService;
    protected readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService, IConfiguration configuration, ILogger<TenantBaseController> logger) : base(configuration, logger)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    // Tenant service user login procedure by credentials validation 
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        var result = await _userService.ValidateUserCredentialsAsync(request.Username, request.Password);

        if (!result.IsSuccess)
        {
            return Unauthorized(result.ErrorMessage);
        }

        var tokenpair = await _tokenService.CreateTokenPair(request.Username, "Admin");

        var loginResponse = new UserLoginResponse
        {
            AccessToken = tokenpair.AccessToken,
            RefreshToken = tokenpair.RefreshToken,
            Success = true,
            Message = "Login successful"
        };

         var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/auth/refresh",
            MaxAge = TimeSpan.FromDays(7),
            IsEssential = true
        };

        Response.Cookies.Append("refreshToken", tokenpair.RefreshToken, cookieOptions);

        return Ok(loginResponse);
    }

    // Tenant service user logout procedure by refresh token invalidation
    [HttpPost("refresh")]
    [AllowAnonymous]    
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var tokenpair = await _tokenService.RefreshTokenPairAsync(request.AccessToken, request.RefreshToken);





        return Ok(tokenpair);
    }

    [HttpPost("logout")]
    [AllowAnonymous]    
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        // 1. Invalida il Refresh Token nel Database/Cache
        var success = await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        
        if (!success)
        {
            return BadRequest("Token non valido o già scaduto.");
        }

        // 2. Se usi i Cookie HttpOnly, cancella il cookie impostando la data passata
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logout effettuato con successo." });
    }

}
