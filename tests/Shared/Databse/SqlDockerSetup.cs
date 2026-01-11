using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace IdentityService.Tests.Shared.Databse;

public class SqlDockerSetup : IDisposable
{
    private readonly string _containerName = "TestSqlServer";
    private readonly string _portSql = "1500";
    private readonly string _password = "YourStrong@Passw0rd";
    public string ConnectionString { get; private set; }

    public SqlDockerSetup()
    {
        ConnectionString = $"Server=localhost,{_portSql};User Id=sa;Password={_password};TrustServerCertificate=true;";

        ControllerContainer().Wait();
    }

    private async Task ControllerContainer()
    {
        await StartDockerContainer();

        WaitForSqlAvailability();
    }

    private async Task StartDockerContainer()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"run --name {_containerName} --rm -e \"ACCEPT_EULA=Y\" -e \"SA_PASSWORD={_password}\" -p {_portSql}:1433 -d mcr.microsoft.com/mssql/server:2022-latest",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }

            await Task.Delay(500);

            if (process?.ExitCode != 0)
            {
                var errorMessage = process?.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to start SQL Server Docker container: {errorMessage}");
            }
        }
        catch (System.Exception)
        {
            Console.WriteLine("Erro ao iniciar o contêiner Docker");
            throw;
        }
    }

    private void WaitForSqlAvailability()
    {
        var maxAttempts = 10;
        var attempt = 0;
        var connected = false;

        while (attempt < maxAttempts && !connected)
        {
            try
            {
                attempt++;
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("CREATE DATABASE TestDb;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    ConnectionString = $"Server=localhost,{_portSql};Database=TestDb;User Id=sa;Password={_password};TrustServerCertificate=true;";
                    connected = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect to the SQL Server Docker container: {e.Message}");
                Thread.Sleep(2000); // Esperar 2 segundos antes de tentar novamente
            }
        }

        if (!connected)
        {
            throw new InvalidOperationException("Could not connect to the SQL Server Docker container.");
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"stop {_containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao parar o container: {ex.Message}");
        }

        try
        {
            var removeProcessStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"rm {_containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(removeProcessStartInfo))
            {
                process?.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao remover o container: {ex.Message}");
        }
    }
}
