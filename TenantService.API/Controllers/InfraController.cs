using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantService.Application;

namespace TenantService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/infra")]
public class InfraController : TenantBaseController
{
    protected readonly IInitService _initService;
    protected readonly IUserService _userService;

    public InfraController(IInitService initService, IUserService userService, 
        IConfiguration configuration, ILogger<TenantBaseController> logger) : base(configuration, logger)
    {
        _initService = initService;
        _userService = userService;
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

        TenantInfoResult data = new TenantInfoResult($"{title}", $"{version}", "Servizi di gestione applicazioni Multi-Tenant", $"{copyright}");

        _logger.LogInformation(title, version, copyright);

        return Ok(data);
    }

    [HttpPost("initialize")]
    [AllowAnonymous]
    public async Task<IActionResult> Initialize()
    {










        
            return Ok();    
    }

    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] UserOperationRequest request)
    {
        _logger.LogWarning($"Adding new User {request.User.ToString()}");

        var userId = await _userService.AddUserAsync(request.User);

        return Ok(new { Id = userId });
    }


    // [HttpPost("login")]
    // public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    // {
    //     _logger.LogWarning($"Attempting to login User {request.Username}");

    //     var isValid = await _userService.ValidateUserCredentialsAsync(request.Username, request.Password);

    //     if (!isValid)
    //     {
    //         return Unauthorized();
    //     }

    //     return Ok(new { Message = "Login successful." });
    // }

}
