using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TenantService.Application;
using TenantService.Application.DTOs;
using TenantService.Application.Services;
using TenantService.Domain;
using TenantService.Domain.Entities;
using TenantService.Domain.Enums;

namespace TenantService.Infrastructure;

public class InitService : ApplicationService, IInitService
{

    private readonly TenantDbContext _context;
    private readonly IUserService _usersvc;

    private readonly IStringLocalizer<InitService> _stringLocalizer;

    public InitService(TenantDbContext context, IUserService userSvc, IConfiguration config, ILogger<InitService> logger) : base(config, logger)
	{
        _context = context;
        _usersvc = userSvc;
	}

    public async Task<String> InitializeDatabaseAsync (CancellationToken cancellationToken = default)
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
                
                // Definisci il percorso della cartella contenente gli script
                string scriptsFolder = Path.Combine(AppContext.BaseDirectory, "Scripts");

                if (!Directory.Exists(scriptsFolder))
                {
                    return $"Cartella degli script non trovata in: {scriptsFolder}";
                }

                // Recupera tutti i file .sql ordinati alfabeticamente (es. 01_..., 02_...)
                var sqlFiles = Directory.GetFiles(scriptsFolder, "*.sql").OrderBy(f => f);

                // Apriamo una transazione per garantire che tutti gli script vengano eseguiti con successo
                await using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var file in sqlFiles)
                {
                    _logger.LogInformation("Esecuzione dello script: {FileName}", Path.GetFileName(file));
                    
                    // Legge l'intero contenuto del file SQL
                    string ddlScript = await File.ReadAllTextAsync(file);

                    // Esegue il DDL nativo tramite EF Core
                    await _context.Database.ExecuteSqlRawAsync(ddlScript);
                }

                // Conferma la transazione se non ci sono stati errori
                await transaction.CommitAsync();
                schemaCreatedNow = true;
                
                _logger.LogInformation("Schemi e tabelle creati con successo.");
            }

            if (schemaCreatedNow)
            {
                _logger.LogInformation("Inserimento dati di primo avvio (Seed)...");
                await SeedInitialDataAsync();
            }

            // create user admin if not exists
            try
            {
                var adminUser = await _usersvc.GetUserByUsernameAsync("admin", cancellationToken);

                if (adminUser != null)
                {
                    _logger.LogInformation("Admin user already exists.");                   
                }
            }
            catch (UserNotFoundException ufex)
            {
                _logger.LogWarning("Admin user not found in database. Creating default admin user...");

                await _usersvc.AddUserAsync(new UserDto() 
                        { Id= Guid.NewGuid().ToString(), 
                        Username = "admin", 
                        FullName = "Administrator", 
                        Email = "admin@example.com",
                        Password = "Admin@99$",
                        Admin = true,
                        Status = (int)UserStatus.Active

                        }, cancellationToken);
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
            _context.DbUpdates.Add(new DbUpdate { Version ="1.0.0", AppliedAt = DateTime.UtcNow });
            // Aggiungi qui altri dati iniziali (es. Utente Admin, Ruoli, ecc.)
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Dati di Seed inseriti correttamente.");
        }
    }
}
