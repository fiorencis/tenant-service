
using TenantService.Application.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Enums;

namespace TenantService.Application;

public static class TenantMapping
{
    public static Tenant ToCreateTenant(this TenantDto dto) => new()
    {
        Code = dto.Code,
        Name = dto.Name,
        TaxCode = dto.TaxCode,
        Email = dto.Email,
        SubscriptionDate = dto.SubscriptionDate,
        DisposalDate = dto.DisposalDate,
        Notes = dto.Notes,
        Status = (TenantStatus)dto.Status
    };

    public static Tenant ToUpdateTenant(this TenantDto dto, Tenant tenant)
    {
        tenant.Code = dto.Code;
        tenant.Name = dto.Name;
        tenant.TaxCode = dto.TaxCode;
        tenant.Email = dto.Email;
        tenant.SubscriptionDate = dto.SubscriptionDate;
        tenant.DisposalDate = dto.DisposalDate;
        tenant.Notes = dto.Notes;
        tenant.Status = (TenantStatus)dto.Status;

        return tenant;
    }

    public static TenantDto ToTenantDto(this Tenant tenant) => new()
    {
        Id = tenant.Id.ToString(),
        Code = tenant.Code,
        Name = tenant.Name,
        TaxCode = tenant.TaxCode,
        Email = tenant.Email,
        SubscriptionDate = tenant.SubscriptionDate,
        DisposalDate = tenant.DisposalDate,
        Notes = tenant.Notes,
        Status = (int)tenant.Status
    };

    public static TenantDto ToCodeNameDto(this Tenant tenant) => new()
    {
        Id = tenant.Id.ToString(),
        Code = tenant.Code,
        Name = tenant.Name,
        Status = (int)tenant.Status
    };
}