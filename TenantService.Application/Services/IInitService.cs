namespace TenantService.Application;

public interface IInitService : IApplicationService
{
    Task<String> InitializeDatabase (CancellationToken cancellationToken = default);
}
