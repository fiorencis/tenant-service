using TenantService.Domain.Entities;

namespace TenantService.Domain;

public class License
{

    public Guid Id { get; set; }
    public Tenant Tenant { get; set; } = default!;
    public string SerialNumber { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int MaxUsers { get; set; }


}
