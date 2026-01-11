using IdentityService.Tests.Shared.Databse;
using IdentityService.Infrastructure.Data;

namespace IdentityService.Tests.Domain.Services;

public abstract class BaseServiceTests : IDisposable
{
    protected ApplicationDbContext _context;
    private readonly TestDatabaseMemoryFixtures _dbFixture;

    protected BaseServiceTests()
    {
        _dbFixture = new TestDatabaseMemoryFixtures();
        _context = _dbFixture.Context;
    }

    protected async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        _context?.Dispose();
        _dbFixture.Dispose();
        GC.SuppressFinalize(this);
    }
}

internal class TestDatabaseMemoryFixture : TestDatabaseMemoryFixtures
{
}
