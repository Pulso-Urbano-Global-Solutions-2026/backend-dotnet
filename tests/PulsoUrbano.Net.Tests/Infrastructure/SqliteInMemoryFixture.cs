using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data;

namespace PulsoUrbano.Net.Tests.Infrastructure;

public sealed class SqliteInMemoryFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteInMemoryFixture()
    {
        // Keep the connection open; in-memory DB is destroyed when the last connection closes.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public DbContextOptions<AppDbContext> CreateOptions()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return options;
    }

    public void Dispose() => _connection.Dispose();
}
