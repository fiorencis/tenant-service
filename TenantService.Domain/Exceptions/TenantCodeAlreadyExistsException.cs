namespace TenantService.Domain.Exceptions;

public sealed class TenantCodeAlreadyExistsException : Exception
{
	public TenantCodeAlreadyExistsException(string tenantCode)
		: base($"Tenant code '{tenantCode}' is already in use.")
	{
		TenantCode = tenantCode;
	}

	public string TenantCode { get; }
}

