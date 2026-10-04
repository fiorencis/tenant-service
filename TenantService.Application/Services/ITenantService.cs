using TenantService.Application.DTOs;

namespace TenantService.Application.Services;

public interface ITenantService : IApplicationService
{
    Task<TenantDto[]> GetTenantsAsync();

    Task<Guid> AddTenantAsync(TenantDto tenant, CancellationToken cancellationToken = default);

    Task<Guid> UpdateTenantAsync(TenantDto tenant, CancellationToken cancellationToken = default);

    Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantDto> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
