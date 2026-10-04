using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TenantService.Application;
using TenantService.Application.DTOs;
using TenantService.Application.Repositories;
using TenantService.Application.Services;
using TenantService.Domain.Entities;
using TenantService.Domain.Exceptions;

namespace TenantService.Infrastructure;

public class TenantService : ApplicationService, ITenantService
{
    private readonly IRepository<Tenant> _tenantRepository;
	private readonly IUnitOfWork _unitOfWork;


    public TenantService(IRepository<Tenant> tenantRepository, IUnitOfWork unitOfWork,  
        IConfiguration config, ILogger<ApplicationService> logger) : base(config, logger)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<TenantDto[]> GetTenantsAsync()
    {
        var tenants = await _tenantRepository.ListAsync();
        TenantDto[] tenantDtos = new TenantDto[tenants.Count()];

		var cnt = 0;

		foreach (var tenant in tenants.OrderBy(c => c.Name))
		{
			tenantDtos[cnt] = tenant.ToCodeNameDto();
			cnt++;
		}

		return tenantDtos;
    }


    public async Task<Guid> AddTenantAsync(TenantDto tenant, CancellationToken cancellationToken = default)
    {
        if (tenant.Id == null || string.IsNullOrWhiteSpace(tenant.Id))
		{
			tenant.Id = Guid.NewGuid().ToString();
		}	

        if (!Guid.TryParse(tenant.Id, out var tenantId))
		{
			tenantId = Guid.NewGuid();
		}

        var normalizedTenantCode = tenant.Code?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTenantCode))
		{
			throw new ArgumentException("Tenant code is required.", nameof(tenant.Code));
		}

        if (await _tenantRepository.ExistsAsync(x => x.Code == normalizedTenantCode, cancellationToken))
		{
			throw new TenantCodeAlreadyExistsException(normalizedTenantCode);
		}

        var newTenant = tenant.ToCreateTenant();
        newTenant.Id = tenantId;
        newTenant.Code = normalizedTenantCode;

        await _tenantRepository.AddAsync(newTenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newTenant.Id;
    }


public async Task<Guid> UpdateTenantAsync(TenantDto tenant, CancellationToken cancellationToken = default)
	{
		var tenantentity = await _tenantRepository.GetByIdAsync(Guid.Parse(tenant.Id), cancellationToken);

		if (tenantentity == null)
		{
			throw new TenantNotFoundException(tenant.Id);
		}

		var normalizedTenantCode = tenant.Code?.Trim();

		if (string.IsNullOrWhiteSpace(normalizedTenantCode))
		{
			throw new ArgumentException("Tenant code is required.", nameof(tenant));
		}

		if (await _tenantRepository.ExistsAsync(x => x.Code == normalizedTenantCode && x.Id != tenantentity.Id, cancellationToken))
		{
			throw new TenantCodeAlreadyExistsException(normalizedTenantCode);
		}

		var updTenant = tenant.ToUpdateTenant(tenantentity);
		await _tenantRepository.UpdateAsync(updTenant, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return tenantentity.Id;
	}


    public async Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);

		if (tenant == null)
		{
			throw new TenantNotFoundException(tenantId);
		}

		await _tenantRepository.DeleteAsync(tenant, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return true;
    }


    public async Task<TenantDto> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
        {
            throw new TenantNotFoundException(tenantId);
        }

        return tenant.ToTenantDto();
    }
}
