# Stage 1: Build and Publish the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire repository so referenced projects keep their relative paths
COPY . .
WORKDIR /src/TenantService.API
RUN dotnet restore

# Build and publish for production
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final lightweight runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configures the default port and exposes the traffic
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start command (replace TenantService.API.dll with the actual name of your DLL)
ENTRYPOINT ["dotnet", "TenantService.API.dll"]

# Now you can build and run the Docker image with the following commands:
# docker build -t tenant-service .