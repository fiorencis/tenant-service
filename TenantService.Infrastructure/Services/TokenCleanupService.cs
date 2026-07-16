using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TenantService.Infrastructure.Services;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Executing 'Token Cleanup Service' on {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");

        try 
        {

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), ct);

                _logger.LogInformation($"Deleting elapsed tokens...");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

                var expired = db.RefreshTokens
                    .Where(r => r.ExpiresAt < DateTime.UtcNow || r.IsRevoked);

                db.RefreshTokens.RemoveRange(expired);
                await db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
