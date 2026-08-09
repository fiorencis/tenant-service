using TenantService.API.Controllers.Core;

namespace TenantService.API;

public class ChangePasswordRequest : ApiRequestBase
{
    public string Username { get; set; } = string.Empty;
    
    public string NewPassword { get; set; } = string.Empty; 

    public string OldPassword { get; set; } = string.Empty; 

    public string OriginalHash { get; set; } = string.Empty;
}
