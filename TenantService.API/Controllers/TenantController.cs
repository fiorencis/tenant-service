using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TenantService.Application;
using TenantService.Application.Services;


namespace TenantService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/app")]
public class TenantController : TenantBaseController
{
    protected readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService, IConfiguration configuration, 
        ILogger<TenantBaseController> logger) : base(configuration, logger)
    {
        _tenantService = tenantService;
    }

     [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        var tenants = await _tenantService.GetTenantsAsync();
        return Ok(new { tenants = tenants, success = true, message = "Tenants list" });
    }


    [HttpPost("tenants")]
    public async Task<IActionResult> AddTenant([FromBody] TenantOperationRequest request)
    {
        _logger.LogWarning($"Adding new Tenant {request.Tenant.Name}");

        var tenantId = await _tenantService.AddTenantAsync(request.Tenant);

        return Ok(new { Id = tenantId });
    }

  [HttpPut("tenants/{id}")]
    public async Task<IActionResult> UpdateTenant([FromBody] TenantOperationRequest request)
    {
        _logger.LogWarning($"Updating Tenant {request.Tenant.Name}");

        await _tenantService.UpdateTenantAsync(request.Tenant);

        return Ok(request.Tenant);
    }

    [HttpDelete("tenants")]
    public async Task<IActionResult> DeleteTenant([FromRoute] TenantOperationRequest request)
    {
               _logger.LogWarning($"Deleting Tenant {request.Tenant.Name}");

        await _tenantService.DeleteTenantAsync(Guid.Parse(request.Tenant.Id));

        return Ok();
    }

    [HttpPost("get-tenant-by-id")]
    public async Task<IActionResult> GetTenantById([FromBody] TenantOperationRequest request)
    {
        _logger.LogInformation($"Getting Tenant {request.Tenant.Id}");

        var tenant = await _tenantService.GetTenantByIdAsync(Guid.Parse(request.Tenant.Id));

        return Ok(tenant);
    }



}
