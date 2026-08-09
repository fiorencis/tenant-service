namespace TenantService.API.Controllers.Core;

public abstract class ApiResponseBase
{
    public bool Success { get; set; } = true;

    public string Message { get; set; } = string.Empty;

}