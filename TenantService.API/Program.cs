using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TenantService.Infrastructure;
using TenantService.Infrastructure.Extensions;
using TenantService.Application.Extensions;
using TenantService.Infrastructure.Services;
using Scalar.AspNetCore;
using TenantService.API.Infrastructure;
using TenantService.Application;
using DotNetEnv;

// create the web application builder
var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file in development
if (builder.Environment.IsDevelopment())
{
    Env.Load("../.env");
}   

// Add environment variables to the configuration
builder.Configuration.AddEnvironmentVariables();

// gets the configuration file settings
IConfiguration config = builder.Configuration;

//Gets Connection String from environment variable or appsettings.json
var conStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
           ?? config.GetConnectionString("DefaultConnection");

// Initialize Serilog logger
var logsec = config.GetSection("Logging");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File($"{logsec["LogPath"]}/tnt-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger.Warning($"Logging initialized...");

var jwt = config.GetSection("Jwt");

var secret = Environment.GetEnvironmentVariable("TOKEN_KEY")
    ?? jwt["Key"]
    ?? "supersecretkey1234567890!@#$%^&*()@@_$QuLoW%qwerty&potrimao99@###][";

Log.Logger.Warning($"Used JWT Secret Key: {secret}...");
var keyBytes = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
     options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Imposta a true in produzione
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true, // Imposta a true in produzione
        ValidAudience = jwt["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero // Rimuove il ritardo di tolleranza di default (5 min)
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Logger.Warning($"!!! Autenticazione fallita: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// add OpenApi services
builder.Services.AddOpenApi(options =>
{
    // Aggiunge la definizione JWT direttamente nel documento OpenAPI
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{

    // options.AddPolicy("AngularAppPolicy", policy =>
    // {
    //     policy
    //         .WithOrigins("http://localhost:4200")
    //         .AllowCredentials()
    //         .WithHeaders("Authorization", "Content-Type")
    //         .WithMethods("GET", "OPTIONS");
    // });

    options.AddPolicy("AngularAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5040") // L'URL del tuo frontend
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();        
    });
});

builder.Host.UseSerilog();

// Mappa e registra la sezione "PasswordSettings"
builder.Services.Configure<PasswordSettings>(
    builder.Configuration.GetSection("PasswordSettings"));
builder.Services.Configure<DataFoldersOptions>(
    builder.Configuration.GetSection("DataFolders"));

builder.Services.AddDbContext<TenantDbContext>(options => options.UseNpgsql(conStr));
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

builder.Services.AddHostedService<TokenCleanupService>();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();
//builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine(
        $">>> HTTP {context.Request.Method} {context.Request.Path}");

    await next();

    Console.WriteLine(
        $"<<< HTTP {context.Response.StatusCode} {context.Request.Method} {context.Request.Path}");
});

app.UseRouting();
app.UseCors("AngularAppPolicy");

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment()) {
    app.UseHttpsRedirection();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Log.Logger.Debug("USING SCALAR / OPENAPI!!!");
    app.MapOpenApi();
    
    // Genera l'interfaccia grafica Scalar invece di Swagger
    app.MapScalarApiReference();     

}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var supportedCultures = new[]
{
    "en-US",
    "it-IT"
};

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);


app.Run();
            