using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration: env vars override appsettings ---
builder.Configuration.AddEnvironmentVariables();

// --- DbContext (Oracle) — env-var connection string (no secrets committed) ---
string oracleConn =
    $"User Id={Environment.GetEnvironmentVariable("DB_USER") ?? "system"};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASS") ?? "oracle"};" +
    $"Data Source={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"}:" +
    $"{Environment.GetEnvironmentVariable("DB_PORT") ?? "1521"}/" +
    $"{Environment.GetEnvironmentVariable("DB_SERVICE") ?? "XEPDB1"};";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseOracle(oracleConn));

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // configured fully in N-27

// --- AlertaService — registered in N-22 ---
// builder.Services.AddScoped<AlertaService>();

// --- CORS: allows mobile and Java API to call ---
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// MIDDLEWARE ORDER — do not reorder
// 1. app.UseMiddleware<GlobalExceptionMiddleware>();  // N-19

// 2. Swagger — always on for GS demo
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pulso Urbano .NET v1"));

// 3. HTTPS redirect only when a TLS endpoint is actually configured
if (app.Environment.IsDevelopment() &&
    app.Urls.Any(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
{
    app.UseHttpsRedirection();
}

// 4. CORS
app.UseCors();

// 5. app.UseMiddleware<JwtValidationMiddleware>();  // N-17
app.UseRouting();
app.MapControllers();

// Auto-migrate + seed on startup in Development (N-10/N-11)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db);
}

app.Run();

public partial class Program { } // for WebApplicationFactory<Program> (N-29)
