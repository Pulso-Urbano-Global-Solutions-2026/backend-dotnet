using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PulsoUrbano.Net.Data;

namespace PulsoUrbano.Net.Tests.Infrastructure;

public class PulsoWebAppFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSecret = "test-secret-key-for-integration-tests-32chars!!";

    private readonly SqliteConnection _connection;

    public PulsoWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // keep alive — in-memory DB dies when last connection closes
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace Oracle DbContext with SQLite in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_connection));

            // Override JWT_SECRET so middleware accepts test-minted tokens
            Environment.SetEnvironmentVariable("JWT_SECRET", TestJwtSecret);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed once after the host is built (factory owns lifecycle)
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        DataSeeder.SeedAsync(db).GetAwaiter().GetResult();

        return host;
    }

    /// <summary>Creates an HttpClient pre-authorized with a valid JWT for protected routes.</summary>
    public HttpClient CreateAuthenticatedClient(string role = "USER")
    {
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [
                new Claim("usuarioId", "1"),
                new Claim("email",     "test@gs.fiap.com"),
                new Claim("role",      role)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", handler.WriteToken(token));
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
