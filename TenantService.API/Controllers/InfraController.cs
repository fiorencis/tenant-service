using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql.Internal;
using TenantService.Application;

namespace TenantService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/infra")]
public class InfraController : TenantBaseController
{
    protected readonly IInitService _initService;
    protected readonly IUserService _userService;

    private readonly PasswordSettings _passwordSettings;


    public InfraController(IInitService initService, IUserService userService, 
        IConfiguration configuration, ILogger<TenantBaseController> logger, 
        IOptions<PasswordSettings> passwordOptions) : base(configuration, logger)
    {
        _initService = initService;
        _userService = userService;
        _passwordSettings = passwordOptions.Value;
    }
        
    [HttpGet("info")]
    [AllowAnonymous]
    public async Task<IActionResult> Info()
    { 
        _logger.LogDebug("Service Info request");

        var assembly = Assembly.GetExecutingAssembly();

        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        var version = assembly.GetName().Version;
        var title = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        var verstr = version?.ToString(3) ?? "1.0.0";

        TenantInfoResult data = new TenantInfoResult($"{title}", $"{verstr}", "Servizi di gestione applicazioni Multi-Tenant", $"{copyright}");

        _logger.LogInformation(title, verstr, copyright);

        return Ok(data);
    }

    [HttpPost("initialize")]
    [AllowAnonymous]
    public async Task<IActionResult> Initialize()
    {

        var result = await _initService.InitializeDatabaseAsync();



        
            return Ok(result);    
    }

    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] UserOperationRequest request)
    {
        _logger.LogWarning($"Adding new User {request.User.Username}");

        var userId = await _userService.AddUserAsync(request.User);

        return Ok(new { Id = userId });
    }

    [HttpPost("update-user")]
    public async Task<IActionResult> UpdateUser([FromBody] UserOperationRequest request)
    {
        _logger.LogWarning($"Updating User {request.User.Username}");

        await _userService.UpdateUserAsync(request.User);

        return Ok();
    }


    [HttpPost("delete-user")]
    public async Task<IActionResult> DeleteUser([FromBody] UserOperationRequest request)
    {
        _logger.LogWarning($"Deleting User {request.User.Username}");

        await _userService.DeleteUserAsync(Guid.Parse(request.User.Id));

        return Ok();
    }

    [HttpPost("get-user")]
    public async Task<IActionResult> GetUser([FromBody] UserOperationRequest request)
    {
        _logger.LogInformation($"Getting User {request.User.Username}");

        var user = await _userService.GetUserByUsernameAsync(request.User.Username);

        return Ok(user);
    }

    [HttpGet("{id}/avatar")]
    public async Task<IActionResult> GetAvatar(Guid id)
    {
        _logger.LogWarning(">>> GETAVATAR chiamato: {Method} {Id}", Request.Method, id);

         var path = await _userService.GetUserImagePath(id);

        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
        {
            _logger.LogWarning("Avatar non trovato per utente {Id}", id);
            return NotFound();
        }

        return PhysicalFile(path, "image/webp");
    }

    [HttpPut("{id}/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(
        Guid id,
        [FromForm] IFormFile avatar,
        CancellationToken cancellationToken)
    {
        if (avatar == null || avatar.Length == 0)
        {
            return BadRequest("Avatar is required.");
        }

        // if (!string.Equals(avatar.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase))
        // {
        //     return BadRequest("Only WebP images are supported.");
        // }

        var path = await _userService.GetUserImagePath(id);
        var directory = Path.GetDirectoryName(path);

        _logger.LogWarning(">>> UPLOADAVATAR called for user {Id}, saving to path: {Path}", id, path);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        Directory.CreateDirectory(directory);
        var temporaryPath = path + ".tmp";

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true))
            {
                await avatar.CopyToAsync(stream, cancellationToken);
            }

            System.IO.File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (System.IO.File.Exists(temporaryPath))
            {
                System.IO.File.Delete(temporaryPath);
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}/avatar")]
    public async Task<IActionResult> DeleteAvatar(Guid id, CancellationToken cancellationToken)
    {
        var path = await _userService.GetUserImagePath(id);

        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        System.IO.File.Delete(path);

        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        _logger.LogInformation($"Changing password for User {request.Username}");

        // if the password does not meet the requirements, return a bad request with the error message
        if (!PasswordHelper.ValidatePassword(request.NewPassword, _passwordSettings, out string errorMessage))
        {
            return BadRequest(errorMessage);
        }

        var user = await _userService.GetUserByUsernameAsync(request.Username);

        if (user == null)
        {
            return NotFound($"User {request.Username} not found.");
        }

        user.Password = request.NewPassword;

        await _userService.UpdateUserAsync(user);

        return Ok();
    }


    [HttpPost("verify-password")]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
    {
        _logger.LogInformation($"Verifying password for User {request}");

        try 
        {
            var isValid = await _userService.VerifyUserPasswordAsync(request.UserId, request.Password);

            return Ok(new { IsValid = isValid, success = true, message = isValid ? "Password is valid." : "Password is invalid." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while verifying the password.");
        }

    }

    [HttpPost("validate-password")]
    public async Task<IActionResult> ValidatePassword([FromBody] VerifyPasswordRequest request)
    {
        _logger.LogInformation($"Validating password for User {request}");

        try
        {
            if (!PasswordHelper.ValidatePassword(request.Password, _passwordSettings, out string errorMessage))
            {
                return Ok(new { IsValid = false, success = true, message = errorMessage });
            } 

            return Ok(new { IsValid = true, success = true, message = "Password is valid." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while validating the password."); 
        }
        
    }



    [AllowAnonymous]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("OK");
    }

}
