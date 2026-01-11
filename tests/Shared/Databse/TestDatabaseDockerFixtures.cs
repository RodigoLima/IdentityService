using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Tests.Shared.Databse;

public class TestDatabaseDockerFixtures : IDisposable
{
  public ApplicationDbContext DbContext { get; private set; }
  private readonly SqlDockerSetup _sqlDockerSetup;

  public TestDatabaseDockerFixtures()
  {
    _sqlDockerSetup = new SqlDockerSetup();  // Inicia o Docker
    var serviceProvider = new ServiceCollection()
        .AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(_sqlDockerSetup.ConnectionString))
        .BuildServiceProvider();

    DbContext = serviceProvider.GetService<ApplicationDbContext>()!;

    DbContext.Database.EnsureDeleted();
    DbContext.Database.EnsureCreated();
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    DbContext.Dispose();
    _sqlDockerSetup.Dispose();
  }
}
