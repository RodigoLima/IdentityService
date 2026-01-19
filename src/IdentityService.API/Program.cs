using IdentityService.Api.Configuration;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Domain.Configuration;
using IdentityService.Domain.Interfaces.Security;
using IdentityService.Domain.Services.Security;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Observabilidade - Métricas Prometheus
app.UseMetricServer();
app.UseHttpMetrics();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
