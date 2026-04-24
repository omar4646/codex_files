using Microsoft.EntityFrameworkCore;

namespace MediBookDesktop.Data;

public class AppDbContextFactory
{
    private readonly string _connectionString;

    public AppDbContextFactory(string? databasePath = null)
    {
        _connectionString = $"Data Source={databasePath ?? AppDbContext.DefaultDatabasePath}";
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
