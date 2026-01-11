using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prometheus;
using IdentityService.API.Authorization;
using IdentityService.API.Filters;
using IdentityService.API.Logs;
using IdentityService.API.Middlewares;
using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Domain.Configuration;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;
using IdentityService.Domain.Interfaces.Security;
using IdentityService.Domain.Services;
using IdentityService.Domain.Services.Security;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Data.Repositories;
using IdentityService.Infrastructure.Helpers;
using static IdentityService.API.Constants.AppConstants;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// JWT Configuration
var jwtKeyConfig = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKeyConfig))
    throw new InvalidOperationException("Jwt:Key configuration is missing or empty.");

builder.Services.Configure<TokenConfiguration>(builder.Configuration.GetSection("Jwt"));

// Authentication & Authorization
builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtKeyConfig)),
        RequireExpirationTime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicyWithPermission(Policies.Admin, AccessLevel.Admin)
           .AddPolicyWithPermission(Policies.User, AccessLevel.User)
           .AddPolicyWithPermission(Policies.Guest, AccessLevel.Guest);
}).AddAuthorizationBuilder();

// Controllers
builder.Services.AddControllers(options => options.Filters.Add<UserFilter>())
    .AddNewtonsoftJson(options =>
    {
        var settings = options.SerializerSettings;
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.FloatFormatHandling = FloatFormatHandling.DefaultValue;
        settings.FloatParseHandling = FloatParseHandling.Double;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
        settings.Culture = new CultureInfo("en-US");
        settings.Converters.Add(new StringEnumConverter());
        settings.ContractResolver = new DefaultContractResolver() 
        { 
            NamingStrategy = new SnakeCaseNamingStrategy() 
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Identity Service API", 
        Version = "v1",
        Description = "Microserviço de autenticação e gerenciamento de identidade"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.CustomSchemaIds(type =>
    {
        var namingStrategy = new SnakeCaseNamingStrategy();
        return namingStrategy.GetPropertyName(type.Name, false);
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization Header - utilizado com Bearer Authentication. \r\n\r\n Insira 'Bearer' [espaço] e então seu token na caixa abaixo.\r\n\r\nExemplo: (informar sem as aspas): 'Bearer 1234sdfgsdf' ",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Health Checks & Metrics
builder.Services.AddHealthChecks();

// AutoMapper
builder.Services.AddAutoMapper((sp, cfg) =>
{
    cfg.AllowNullDestinationValues = true;
    cfg.AllowNullCollections = true;
    cfg.ConstructServicesUsing(sp.GetService);
}, Assembly.GetAssembly(typeof(BaseModel)));

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information
}));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));

// Cache
builder.Services.AddMemoryCache();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Domain Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Application Services
builder.Services.AddScoped<IUserApplicationService, UserApplicationService>();
builder.Services.AddScoped<ITokenApplicationService, TokenApplicationService>();

// Authorization
builder.Services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();

// Filters
builder.Services.AddScoped<IAuthorizationFilter, UserFilter>();
builder.Services.AddScoped(x => new UserData());

// Build App
var app = builder.Build();

// Middleware Pipeline
app.UseHealthChecks("/health");
app.UseHttpMetrics();
app.MapMetrics();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1"));

// Database Migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Aplicando migrações do banco de dados...");
        await MigrationHelper.RunMigrationsAsync(context);
        logger.LogInformation("Migrações aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao aplicar migrações: {Message}", ex.Message);
        if (!app.Environment.IsProduction())
        {
            throw;
        }
    }
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
