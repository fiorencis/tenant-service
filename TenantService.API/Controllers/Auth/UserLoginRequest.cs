using TenantService.API.Controllers.Core;

namespace TenantService.API;

public class UserLoginRequest : ApiRequestBase
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
