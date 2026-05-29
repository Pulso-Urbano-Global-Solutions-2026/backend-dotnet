using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PulsoUrbano.Net.Data;
using PulsoUrbano.Net.Exceptions;
using PulsoUrbano.Net.Middleware;
using PulsoUrbano.Net.Services;
using PulsoUrbano.Net.Validators;

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Pulso Urbano .NET",
        Version     = "v1",
        Description = "API secundária — histórico de alertas e estatísticas ambientais de São Paulo."
    });

    // Bearer button — professor inserts the Java-issued JWT and tests protected endpoints live
    var bearer = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Token JWT emitido pelo Java API. Ex: Bearer eyJhbGci..."
    };
    c.AddSecurityDefinition("Bearer", bearer);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations();

    var xml = Path.Combine(AppContext.BaseDirectory, "PulsoUrbano.Net.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

// --- Domain services (N-22) ---
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IEstatisticasService, EstatisticasService>();
builder.Services.AddValidatorsFromAssemblyContaining<AlertaCreateDTOValidator>();
builder.Services.AddFluentValidationAutoValidation(); // returns 400 on invalid model automatically

// --- CORS: allows mobile and Java API to call ---
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// MIDDLEWARE ORDER — do not reorder
// 1. Global exception handler (must be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

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

// 5. JWT validation (public-route bypass inside the middleware — N-17/N-18)
app.UseMiddleware<JwtValidationMiddleware>();
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
