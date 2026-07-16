using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TenantService.Application;
using TenantService.Domain.Entities;
using TenantService.Infrastructure.Services;

namespace TenantService.Infrastructure;

public class InitService : ApplicationService, IInitService
{

    private readonly TenantDbContext _context;
    private readonly IUserService _usersvc;

    public InitService(TenantDbContext context, UserService userSvc, IConfiguration config, ILogger<InitService> logger) : base(config, logger)
	{
        _context = context;
        _usersvc = userSvc;
	}

    public async Task<String> InitializeDatabase (CancellationToken cancellationToken = default)
	{
        try
        {
            _logger.LogInformation("Avvio procedura di inizializzazione del database.");

		    var dbcreateSvc = _context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

             if (dbcreateSvc == null)
            {
                return "Impossibile ottenere il servizio RelationalDatabaseCreator.";
            }

            bool dbCreatedNow = false;
            bool schemaCreatedNow = false;

            if (!await dbcreateSvc.ExistsAsync())
            {
                _logger.LogInformation("Database non esistente. Creazione in corso...");
                await dbcreateSvc.CreateAsync();
                dbCreatedNow = true;
                _logger.LogInformation("Database creato con successo.");
            }

            if (dbCreatedNow || !await dbcreateSvc.HasTablesAsync())
            {
                _logger.LogInformation("Tabelle non presenti. Esecuzione script DDL / Migrazioni...");
                
                // Genera e applica tutte le tabelle definite nel DbContext
                await dbcreateSvc.CreateTablesAsync();
                schemaCreatedNow = true;
                
                _logger.LogInformation("Schemi e tabelle creati con successo.");
            }

            if (schemaCreatedNow)
            {
                _logger.LogInformation("Inserimento dati di primo avvio (Seed)...");
                await SeedInitialDataAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'inizializzazione del database.");
            return "Errore durante l'inizializzazione del database.";
        }
        
		return string.Empty;
	}

    private async Task SeedInitialDataAsync()
    {
        // Verifica se i dati ci sono già (ulteriore controllo di sicurezza)
        if (!await _context.DbUpdates.AnyAsync())
        {
            _context.DbUpdates.Add(new DbUpdate { Id = 1, AppliedAt = DateTime.Now, Version ="1.0.0" });
            // Aggiungi qui altri dati iniziali (es. Utente Admin, Ruoli, ecc.)
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Dati di Seed inseriti correttamente.");
        }
    }
}
