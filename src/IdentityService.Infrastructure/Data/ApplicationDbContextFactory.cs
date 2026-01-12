using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace IdentityService.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Carrega a configuração do projeto API
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../IdentityService.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Detecta o provider baseado na configuração
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";

        Console.WriteLine($"Using {provider} provider");

        var connectionString = configuration.GetConnectionString("PostgreSql");
        options.UseNpgsql(connectionString, x =>
        {
            x.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new ApplicationDbContext(options.Options);
    }
}
