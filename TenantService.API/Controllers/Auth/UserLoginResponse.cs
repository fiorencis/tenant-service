using TenantService.API.Controllers.Core;

namespace TenantService.API;

public class UserLoginResponse : ApiResponseBase
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    
}
