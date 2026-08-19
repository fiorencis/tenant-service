using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

    protected const string RefreshTokenCookieName = "X-Refresh-Token";

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
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(7),
            IsEssential = true
        };

        Response.Cookies.Append(RefreshTokenCookieName, tokenpair.RefreshToken, cookieOptions);
        _logger.LogDebug(">>> Refresh token cookie {RefreshTokenCookieName}: {RefreshToken} ", RefreshTokenCookieName, tokenpair.RefreshToken);

        return Ok(loginResponse);
    }

    // Tenant service user logout procedure by refresh token invalidation
    // [HttpPost("refresh_old")]
    // [AllowAnonymous]    
    // public async Task<IActionResult> RefreshOld([FromBody] RefreshRequestDto request)
    // {
    //     _logger.LogInformation($"Refreshing token for user with access token: {request.AccessToken} and refresh token: {request.RefreshToken}");

    //     var tokenpair = await _tokenService.RefreshTokenPairAsync(request.AccessToken, request.RefreshToken);

    //     var loginResponse = new UserLoginResponse
    //     {
    //         AccessToken = tokenpair.AccessToken,
    //         RefreshToken = tokenpair.RefreshToken,
    //         Success = true,
    //         Message = "Login successful"
    //     };

    //      var cookieOptions = new CookieOptions
    //     {
    //         HttpOnly = true,
    //         Secure = true,
    //         SameSite = SameSiteMode.Strict,
    //         Path = "/auth/refresh",
    //         MaxAge = TimeSpan.FromDays(7),
    //         IsEssential = true
    //     };

    //     Response.Cookies.Append("X-Refresh-Token", tokenpair.RefreshToken, cookieOptions);

    //     return Ok(loginResponse);
    // }

    [HttpPost("refresh")]
    [AllowAnonymous]    
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        // 1. Estrae il token direttamente dai Cookie inseriti dal browser
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken))
        {
            _logger.LogWarning("Refresh Token mancante nei cookie.");
            _logger.LogInformation($"Request cookies: {string.Join(", ", Request.Cookies.Keys)}");
            return BadRequest(new { Message = "Refresh Token mancante nei cookie." }); // Questo genera il 400 se il cookie non c'è!
        }

        try 
        {
            var tokenpair = await _tokenService.RefreshTokenPairAsync(request.AccessToken, refreshToken);

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

            Response.Cookies.Append(RefreshTokenCookieName, tokenpair.RefreshToken, cookieOptions);

            return Ok(loginResponse);

        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }      

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
        Response.Cookies.Delete(RefreshTokenCookieName);

        return Ok(new { message = "Logout effettuato con successo." });
    }

}
