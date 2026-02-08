using IdentityService.Api.Configuration;
using IdentityService.Api.Middlewares;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Domain.Configuration;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Interfaces.Security;
using IdentityService.Domain.Services.Security;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Prometheus;
using Serilog;
using Serilog.Events;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "IdentityService")
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/identityservice-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Service}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Iniciando IdentityService...");

    var builder = WebApplication.CreateBuilder(args);
    
    // Usar Serilog
    builder.Host.UseSerilog();

// Configurações
builder.Services.ConfigureSwagger();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.ConfigureJwt(builder.Configuration, builder.Environment);

// Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Token Configuration
builder.Services.Configure<TokenConfiguration>(builder.Configuration.GetSection("Jwt"));

// Repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Domain Services (Security)
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

// Application Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IdentityService.Application.Services.TokenService>();

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var vhost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
        if (string.IsNullOrEmpty(vhost) || vhost.Contains(':') || vhost.Contains("Program")) vhost = "/";
        var username = builder.Configuration["RabbitMq:Username"] ?? "admin";
        var password = builder.Configuration["RabbitMq:Password"] ?? "admin123";
        cfg.Host(host, vhost, h =>
        {
            h.Username(username);
            h.Password(password);
        });
    });
});

var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Log.Information("Migrations aplicadas com sucesso");

        var adminEmail = app.Configuration["DefaultAdmin:Email"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var existing = await userRepo.ObterPorEmailAsync(adminEmail);
            if (existing == null)
            {
                var adminPassword = app.Configuration["DefaultAdmin:Password"] ?? "Admin@123";
                var adminId = Guid.NewGuid();
                var admin = new User
                {
                    Id = adminId,
                    Name = "Administrador",
                    Email = adminEmail,
                    Password = passwordHasher.HashPassword(adminPassword),
                    AccessLevel = AccessLevel.Admin,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId,
                    Removed = false
                };
                await userRepo.CriarAsync(admin);
                Log.Information("Usuario administrador padrao criado: {Email}", adminEmail);
            }
        }
    }

    // Middlewares
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
        };
    });

    // Pipeline
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
        c.RoutePrefix = "swagger";
    });

    // Observabilidade - Métricas Prometheus
    app.UseMetricServer();
    app.UseHttpMetrics();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("IdentityService iniciado com sucesso");
    var urls = app.Configuration["ASPNETCORE_URLS"] ?? app.Configuration["urls"] ?? "http://localhost:5093";
    Log.Information("Escutando em: {Urls}", urls);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha ao iniciar IdentityService");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
