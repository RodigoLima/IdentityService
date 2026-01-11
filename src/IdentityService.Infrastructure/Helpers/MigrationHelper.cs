using Microsoft.EntityFrameworkCore;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Infrastructure.Helpers;

public static class MigrationHelper
{
    public static async Task RunMigrationsAsync(ApplicationDbContext context)
    {
        var provider = GetDatabaseProvider(context);
        Console.WriteLine($"Detectado provider: {provider}...");
        Console.WriteLine($"Detectado Database connection: {context.Database.GetDbConnection()}...");
        Console.WriteLine($"Iniciando migrações para {context.Database.GetDbConnection().GetType().Name}...");
        
        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"Pode conectar ao banco: {canConnect}...");
            
            // if (canConnect)
            // {
                Console.WriteLine($"Aplicando migrações para {provider}...");
                
                // Aplicar migrações específicas do provider
                await ApplyMigrationsForProvider(context, provider);
                
                Console.WriteLine("Migrações aplicadas com sucesso!");
            // }
            // else
            // {
            //     Console.WriteLine("Não foi possível conectar ao banco de dados.");
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
            throw;
        }
    }
    
    private static async Task ApplyMigrationsForProvider(ApplicationDbContext context, string provider)
    {
        try
        {
            // Verificar se há migrations pendentes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Aplicando {pendingMigrations.Count()} migration(s) pendente(s)...");
                foreach (var migration in pendingMigrations)
                {
                    Console.WriteLine($"- {migration}");
                }
                
                await context.Database.MigrateAsync();
            }
            else
            {
                Console.WriteLine("Nenhuma migration pendente encontrada. Banco está atualizado.");
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
        {
            Console.WriteLine($"Tabela já existe no PostgreSQL: {ex.MessageText}");
            Console.WriteLine("Marcando migrations como aplicadas...");
            
            // Se a tabela já existe, marcar as migrations como aplicadas sem executá-las
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var allMigrations = context.Database.GetMigrations();
            
            if (!appliedMigrations.Any() && allMigrations.Any())
            {
                // Se não há registro de migrations aplicadas mas as tabelas existem,
                // marca a migration mais recente como aplicada
                var latestMigration = allMigrations.Last();
                Console.WriteLine($"Marcando migration {latestMigration} como aplicada...");
                
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, {1}) ON CONFLICT DO NOTHING",
                    latestMigration, "8.0.0");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro específico ao aplicar migrations: {ex.Message}");
            throw;
        }
        
        Console.WriteLine($"Migrações aplicadas para {provider}");
    }

    private static string GetDatabaseProvider(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        return connection.GetType().Name switch
        {
            "SqlConnection" => "SQL Server",
            "NpgsqlConnection" => "PostgreSQL",
            _ => "Desconhecido"
        };
    }
}
