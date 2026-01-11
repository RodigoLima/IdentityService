using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityService.Api.Configuration;

public static class JwtConfiguration
{
    public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() 
            ?? throw new InvalidOperationException("Configuração JWT não encontrada.");

        var disableJwtValidation = environment.IsDevelopment() 
            && configuration.GetValue<bool>("Development:DisableJwtValidation", false);

        services.AddAuthentication(options =>
        {
            if (disableJwtValidation)
            {
                // Em dev com flag ativa, usa esquema que sempre autentica
                options.DefaultAuthenticateScheme = "DevelopmentBypass";
                options.DefaultChallengeScheme = "DevelopmentBypass";
            }
            else
            {
                // Produção ou dev com validação ativa: usa JWT normal
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        })
        .AddScheme<AuthenticationSchemeOptions, DevelopmentBypassHandler>(
            "DevelopmentBypass",
            options => { })
        .AddJwtBearer(options =>
        {
            if (string.IsNullOrEmpty(jwtSettings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey não configurada.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        services.AddAuthorization();
    }
}

// Handler que sempre autentica (apenas para desenvolvimento)
internal class DevelopmentBypassHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevelopmentBypassHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Cria uma identidade com claims básicas para desenvolvimento
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user"),
            new Claim("sub", "00000000-0000-0000-0000-000000000000"),
            new Claim("userId", "00000000-0000-0000-0000-000000000000")
        };

        var identity = new ClaimsIdentity(claims, "DevelopmentBypass");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "DevelopmentBypass");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
