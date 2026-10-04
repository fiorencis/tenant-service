public sealed class TenantNotFoundException : Exception
{
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant with id '{tenantId}' was not found.")
    {
        TenantId = tenantId;
    }

    public TenantNotFoundException(string code) 
        : base($"Tenant with code '{code}' was not found.")
    {
        Code = code;
    }

    public Guid TenantId { get; }
    public string Code { get; }
}