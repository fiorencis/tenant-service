# TenantService

Servizio .NET indipendente per la gestione dei tenant.

## Struttura consigliata

- `TenantService`: progetto principale
- `TenantService.Tests`: test unitari

## Caricamento Packages

> dotnet add package Microsoft.EntityFrameworkCore.Npgsql --project .\tenantService.Api\TenantService.Api.csproj
> dotnet add package Microsoft.EntityFrameworkCore.Design --project .\tenantService.Api\TenantService.Api.csproj

## se EF non è installato
> dotnet tool install --global dotnet-ef

## x logging su file
> dotnet add package Serilog.AspNetCore
> dotnet add package Serilog.Sinks.File

## Avvio

Esempio:

```powershell
cd c:\fiorencis\projects\tenantService\src\TenantService
dotnet run
```

# Build docker image
Execute in TenantService.API folder:
docker build -t dhub.fiorencis.eu/tenant-service:latest .



# Create docker registry on my server
docker run -d -p 5000:5000 --restart=always --name mio-registry registry:2

# Tagga l'immagine
docker tag mia-immagine:latest IP_DEL_TUO_SERVER:5000/mia-immagine:latest

# Push dell'immagine
docker push IP_DEL_TUO_SERVER:5000/mia-immagine:latest