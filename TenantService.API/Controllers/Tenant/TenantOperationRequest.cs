using TenantService.API.Controllers.Core;
using TenantService.Application.DTOs;

namespace TenantService.API;

public class TenantOperationRequest : ApiRequestBase
{
    public TenantDto Tenant { get; set; }
}