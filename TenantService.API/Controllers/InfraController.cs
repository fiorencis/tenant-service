using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    //[AllowAnonymous]
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
}
