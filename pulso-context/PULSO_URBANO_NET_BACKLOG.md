# PULSO URBANO — .NET API BACKLOG
## Sprint: 27/05 → 09/06/2026
## Owner: Felipe Ferrete (RM 562999) + Claude Agent
## API: Secondary · Port 5000 · Domain: Alert History + Statistics
## Stack: ASP.NET Core 10 · EF Core 8 · Oracle.EntityFrameworkCore · Swashbuckle · xUnit

> Boundary recap (absolute): .NET **validates** the JWT issued by Java (shared `JWT_SECRET`), it does **not** generate tokens, calculate scores, ingest satellite data, or own any Java-owned table. It owns the `ALERTA_HISTORICO` + `ZONA_REFERENCIA_NET` lifecycle, aggregated statistics, and the EF Core migration proof.

---

## DEPENDENCY MAP

```
N-01 (scaffold) ─┬─> N-02 (packages) ─┬─> N-04 (Program.cs skeleton)
                 │                    └─> N-03 (test project)
                 │
N-02 ─> N-05 (ZonaReferencia) ──┐
N-02 ─> N-06 (AlertaHistorico) ─┤
N-05 + N-06 ─> N-07 (1:N config) ─> N-08 (Oracle mappings) ─> N-09 (AppDbContext)
N-09 ─> N-10 (InitialCreate migration) ─> N-11 (DataSeeder + verify)

N-02 ─> N-12..N-16 (DTOs)            [parallel, only need project + packages]
N-04 ─> N-17 (JwtValidationMiddleware) ─> N-18 (bypass + role rules)
N-04 ─> N-19 (GlobalExceptionMiddleware)

N-09 + N-12..N-16 ─> N-20 (AlertaService)
N-09 + N-15 ─> N-21 (EstatisticasService)
N-20 + N-21 ─> N-22 (DI registration)

N-20 + N-22 + N-13/14/16 ─> N-23 (AlertaController)
N-21 + N-22 + N-15/16 ─> N-24 (EstatisticasController)
N-09 ─> N-25 (HealthController)
N-23 + N-24 + N-25 ─> N-26 (controller conventions audit)

N-26 ─> N-27 (SwaggerGen config) ─> N-28 (Swagger annotations)

N-03 + N-11 ─> N-29 (WebApplicationFactory fixture)
N-29 + N-23 ─> N-30 (Alerta CRUD happy path tests)
N-29 + N-23 ─> N-31 (confirmar + delete tests)
N-29 + N-24 ─> N-32 (estatisticas + migration smoke test)

(all code tasks pass) ─> N-33 (Dockerfile)
N-33 ─> N-34 (.env.example + env manifest → Clayton handoff)
N-28 + N-34 ─> N-35 (README + Bosak QA issue template)
```

---

## EXECUTION ORDER

```
 1. N-01                         (scaffold — blocks everything)
 2. N-02                         (packages — blocks all code)
 3. N-03, N-04                   (parallel: test project / Program.cs skeleton)
 4. N-05, N-06                   (parallel: entities, both blocked by N-02)
 5. N-12, N-13, N-14, N-15, N-16 (parallel with step 4: DTOs, only need N-02)
 6. N-07                         (blocked by N-05 AND N-06)
 7. N-08                         (blocked by N-07)
 8. N-09                         (blocked by N-08)
 9. N-10                         (blocked by N-09)
10. N-11                         (blocked by N-10)
11. N-17, N-19                   (parallel: middlewares, blocked by N-04)
12. N-18                         (blocked by N-17)
13. N-20                         (blocked by N-09 + DTOs)
14. N-21                         (parallel with N-20, blocked by N-09 + N-15)
15. N-22                         (blocked by N-20 + N-21)
16. N-23, N-24                   (parallel, blocked by N-22 + controllers' DTOs)
17. N-25                         (parallel with N-23/24, blocked by N-09)
18. N-26                         (blocked by N-23 + N-24 + N-25)
19. N-27                         (blocked by N-26)
20. N-28                         (blocked by N-27)
21. N-29                         (blocked by N-03 + N-11)
22. N-30, N-31, N-32             (parallel, blocked by N-29 + respective controllers)
23. N-33                         (blocked by all code tasks building clean)
24. N-34                         (blocked by N-33)
25. N-35                         (blocked by N-28 + N-34) — last
```

---

## TASKS

## N-01 · Project scaffold and solution layout

**Objective:** Create the `PulsoUrbano.Net` Web API project and solution so every later task has a fixed, known structure to write into.

**GS Requirement:** Requisitos técnicos (boas práticas / organização em camadas) — foundation for the 50-pt block.

**Complexity:** S

**Blocks:** N-02, N-03, N-04, every code task
**Requires:** —
**Can parallelize with:** —

### READ BEFORE STARTING
- Project root (must be empty for `pulso-dotnet/`) — confirm no existing `.csproj`.
- `CONTEXT.md` → ".NET API — ESPECIFICAÇÃO COMPLETA" → "Estrutura de projeto" (folder tree is the contract).

### IMPLEMENT
**Create:**
- `PulsoUrbano.Net.sln`
- `PulsoUrbano.Net/PulsoUrbano.Net.csproj` (net10.0, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<GenerateDocumentationFile>true</GenerateDocumentationFile>`)
- Empty folders matching the contract: `Controllers/`, `Models/Entities/`, `Models/DTOs/`, `Data/`, `Data/Migrations/`, `Services/`, `Exceptions/`, `Middleware/`.

**Full implementation spec:**
```bash
dotnet new sln -n PulsoUrbano.Net
dotnet new webapi -n PulsoUrbano.Net --use-controllers -f net10.0
dotnet sln add PulsoUrbano.Net/PulsoUrbano.Net.csproj
```
- Delete the template `WeatherForecast.cs` and `WeatherForecastController.cs`.
- Set `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and `<NoWarn>$(NoWarn);1591</NoWarn>` (silence "missing XML comment" warnings until N-28 fills them).
- **What NOT to do:** do not add NuGet packages here (N-02 owns that), do not write `Program.cs` logic (N-04 owns it).
- ASSUMPTION: `--use-controllers` (not Minimal API). CONTEXT.md allows either; controllers are more demonstrable in Swagger for the GS jury, so we standardize on controllers.

### MIGRATE
N/A (no entity).

### TEST
**Run:**
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Expected:** `Build succeeded. 0 Warning(s) 0 Error(s)`.

**Task is complete only when:** `dotnet build` succeeds and the folder tree matches CONTEXT.md exactly.

### COMMIT
```bash
git add .
git commit -m " chore(scaffold): create PulsoUrbano.Net webapi project and solution (task N-01)"
```

---

## N-02 · NuGet package set
READ CONTEXT.md
**Objective:** Install every dependency the API and tests need so no later task is blocked on a missing package.

**GS Requirement:** Requisitos técnicos (Migration correta depends on EF Core Oracle; documentação depends on Swashbuckle).

**Complexity:** S

**Blocks:** all code tasks
**Requires:** N-01
**Can parallelize with:** —

### READ BEFORE STARTING
- `PulsoUrbano.Net.csproj` — confirm `net10.0`.
- OPUS prompt → "Halting condition" (Oracle EF Core 8.x must match EF Core 8).

### IMPLEMENT
**Modify:** `PulsoUrbano.Net.csproj` (add `PackageReference`s).

**Full implementation spec — exact packages:**
```bash
# API
dotnet add PulsoUrbano.Net package Oracle.EntityFrameworkCore --version 8.23.*
dotnet add PulsoUrbano.Net package Microsoft.EntityFrameworkCore.Design --version 8.0.*
dotnet add PulsoUrbano.Net package Swashbuckle.AspNetCore --version 6.6.*
dotnet add PulsoUrbano.Net package Swashbuckle.AspNetCore.Annotations --version 6.6.*
dotnet add PulsoUrbano.Net package System.IdentityModel.Tokens.Jwt --version 8.*
dotnet add PulsoUrbano.Net package FluentValidation.AspNetCore --version 11.*

# dotnet-ef CLI (global, once)
dotnet tool install --global dotnet-ef --version 8.* || dotnet tool update --global dotnet-ef --version 8.*
```
- ASSUMPTION: `Oracle.EntityFrameworkCore` 8.23.x is the EF Core 8 compatible line. If `dotnet ef migrations add` later throws a provider/EF version mismatch, this is the **only** STOP condition from the prompt — pin to the latest `8.x` that matches the installed EF Core 8 and re-run.
- **What NOT to do:** do not add `Microsoft.EntityFrameworkCore.SqlServer`, `Pomelo`, or any non-Oracle provider in the API project. SQLite for tests is added in N-03 (test project only).

### TEST
```bash
dotnet restore PulsoUrbano.Net/PulsoUrbano.Net.csproj
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Expected:** build succeeds with all packages restored.

**Task is complete only when:** `dotnet list PulsoUrbano.Net package` shows all packages above with 8.x versions and the build is clean.

### COMMIT
```bash
git add PulsoUrbano.Net/PulsoUrbano.Net.csproj
git commit -m " chore(deps): add EF Core Oracle, Swashbuckle, JWT, FluentValidation (task N-02)"
```

---

## N-03 · Test project + base infrastructure

**Objective:** Stand up the xUnit test project with a `WebApplicationFactory` base and SQLite-in-memory strategy so every test task plugs into a ready harness.

**GS Requirement:** Requisitos técnicos (boas práticas / "como vocês testaram as rotas" — apresentação .NET question 11).

**Complexity:** M

**Blocks:** N-29 (and all test tasks)
**Requires:** N-01, N-02
**Can parallelize with:** N-04

### READ BEFORE STARTING
- `PulsoUrbano.Net.csproj` — target framework.
- OPUS prompt → "Test database strategy" (SQLite for unit/service, Oracle for integration).

### IMPLEMENT
**Create:**
- `tests/PulsoUrbano.Net.Tests/PulsoUrbano.Net.Tests.csproj`
- `tests/PulsoUrbano.Net.Tests/Infrastructure/SqliteInMemoryFixture.cs`

**Full implementation spec:**
```bash
dotnet new xunit -n PulsoUrbano.Net.Tests -o tests/PulsoUrbano.Net.Tests -f net10.0
dotnet sln add tests/PulsoUrbano.Net.Tests/PulsoUrbano.Net.Tests.csproj
dotnet add tests/PulsoUrbano.Net.Tests reference PulsoUrbano.Net/PulsoUrbano.Net.csproj
dotnet add tests/PulsoUrbano.Net.Tests package Microsoft.AspNetCore.Mvc.Testing --version 8.0.*
dotnet add tests/PulsoUrbano.Net.Tests package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.*
dotnet add tests/PulsoUrbano.Net.Tests package FluentAssertions --version 6.*
```
- `SqliteInMemoryFixture`: opens a single `SqliteConnection("DataSource=:memory:")`, keeps it open for the fixture lifetime (in-memory DB dies when the last connection closes), exposes a factory method that builds `DbContextOptions<AppDbContext>` against that connection and calls `EnsureCreated()`.
- The full `WebApplicationFactory<Program>` subclass that swaps Oracle for SQLite lives in **N-29** — this task only ships the connection/options fixture and confirms the test project compiles and runs an empty test.
- **What NOT to do:** do not reference Oracle types in tests; SQLite has no `NUMBER(1)` — use `bool` natively (the entity uses `bool Confirmado`, mapped to Oracle `NUMBER(1)` only in `OnModelCreating`, so SQLite is fine).
- ASSUMPTION: `Program.cs` must be made test-discoverable. N-04 adds `public partial class Program { }` at the end so `WebApplicationFactory<Program>` resolves.

### TEST
```bash
dotnet test tests/PulsoUrbano.Net.Tests --logger "console;verbosity=minimal"
```
**Expected:** `Passed! - Failed: 0` (with the xUnit template placeholder test, or one trivial `Assert.True(true)`).

**Task is complete only when:** the test project builds, runs, and the SQLite fixture can create and dispose an in-memory context without throwing.

### COMMIT
```bash
git add tests/ PulsoUrbano.Net.sln
git commit -m " test(infra): add xUnit project with SQLite in-memory fixture (task N-03)"
```

---

## N-04 · Program.cs skeleton + middleware pipeline order + appsettings

**Objective:** Wire the DI container and lock the middleware order so middlewares (N-17, N-19) and controllers register into a stable pipeline.

**GS Requirement:** Requisitos técnicos (arquitetura / boas práticas — apresentação question "por que escolheram essa arquitetura").

**Complexity:** M

**Blocks:** N-17, N-19, N-22 (DI), every controller
**Requires:** N-01, N-02
**Can parallelize with:** N-03

### READ BEFORE STARTING
- OPUS prompt → "PROGRAM.CS MIDDLEWARE PIPELINE ORDER (enforce exactly)".
- `CONTEXT.md` → ".NET appsettings.json" (connection string + Jwt section shape).
- Existing template `Program.cs` (replace it).

### IMPLEMENT
**Create:**
- `appsettings.json`, `appsettings.Development.json`

**Modify:**
- `Program.cs` — full skeleton.

**Full implementation spec — `Program.cs` (skeleton; concrete registrations land in their tasks):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// --- Configuration: env vars override appsettings ---
builder.Configuration.AddEnvironmentVariables();

// --- DbContext (Oracle) — env-resolved connection string (N-09 finalizes) ---
// builder.Services.AddDbContext<AppDbContext>(...)   // wired in N-09/N-22

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // configured fully in N-27
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// MIDDLEWARE ORDER — do not reorder (see prompt)
// 1. app.UseMiddleware<GlobalExceptionMiddleware>();  // N-19
// 2. Swagger (always on for GS demo)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pulso Urbano .NET v1"));
// 3. app.UseHttpsRedirection();   // keep, but see ASSUMPTION
app.UseCors();
// 5. app.UseMiddleware<JwtValidationMiddleware>();    // N-17
app.UseRouting();
app.MapControllers();

// migrations on startup only in Development (N-10/N-11 rely on this)
if (app.Environment.IsDevelopment())
{
    // using var scope = app.Services.CreateScope();
    // scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();  // enable in N-10
}

app.Run();

public partial class Program { } // for WebApplicationFactory<Program> (N-03/N-29)
```
- `appsettings.json` connection string key = `ConnectionStrings:Oracle`. Store a **placeholder** here; the real value is composed from env vars (`DB_USER`, `DB_PASS`, `DB_HOST`, `DB_SERVICE`) in N-09. Never commit a real password.
- ASSUMPTION: in the Docker/cloud deploy the container listens on plain HTTP `:5000` (`ASPNETCORE_URLS=http://+:5000`, per docker-compose in CONTEXT.md). Keep `UseHttpsRedirection()` guarded so it is **not** applied when no HTTPS port is configured, otherwise health checks and the demo over HTTP break. Concretely: only call `app.UseHttpsRedirection()` when `app.Environment.IsDevelopment()` and an HTTPS URL is present.
- **What NOT to do:** do not register services that don't exist yet (leave them commented with the owning task ID); do not add authentication via `AddAuthentication().AddJwtBearer()` — JWT validation is a custom middleware (N-17), not the ASP.NET auth stack, to keep coupling with Java minimal and behavior explicit for the demo.

### TEST
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
dotnet run --project PulsoUrbano.Net &   # then:
curl -s http://localhost:5000/swagger/v1/swagger.json | head -c 200
```
**Expected:** app boots, Swagger JSON is served (empty paths is OK at this stage).

**Task is complete only when:** the app starts on port 5000 and `/swagger` loads in a browser.

### COMMIT
```bash
git add Program.cs appsettings.json appsettings.Development.json
git commit -m "🏗️ feat(bootstrap): Program.cs pipeline order + appsettings (task N-04)"
```

---

## N-05 · ZonaReferencia entity (the "1" side)

**Objective:** Define the parent entity of the 1:N relationship that the GS "relacionamento" criterion is graded on.

**GS Requirement:** Requisitos técnicos → Relacionamento 1:N (lado "1").

**Complexity:** S

**Blocks:** N-07, N-09
**Requires:** N-02
**Can parallelize with:** N-06, N-12..N-16

### READ BEFORE STARTING
- `CONTEXT.md` → ".NET Entidades e relacionamento 1:N obrigatório" (`ZonaReferencia` shape is the contract).

### IMPLEMENT
**Create:** `Models/Entities/ZonaReferencia.cs`

**Full implementation spec:**
```csharp
public class ZonaReferencia
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Municipio { get; set; } = "São Paulo";
    public ICollection<AlertaHistorico> Alertas { get; set; } = new List<AlertaHistorico>();
}
```
- No data annotations here; all mapping is fluent in `OnModelCreating` (N-08) to keep the entity persistence-agnostic.
- **What NOT to do:** do not add lat/lon — that lives in Java's `ZONA_CIDADE`. This is a **reference** table for the .NET domain, intentionally not a cross-API FK (per CONTEXT.md comment "não FK cross-API").

### TEST
Covered by build; relationship asserted in N-32 migration smoke test.
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** the class compiles and `AlertaHistorico` is referenced (forward reference resolved once N-06 lands).

### COMMIT
```bash
git add Models/Entities/ZonaReferencia.cs
git commit -m " feat(entity): add ZonaReferencia (1-side of 1:N) (task N-05)"
```

---

## N-06 · AlertaHistorico entity (the "N" side with FK)

**Objective:** Define the core domain entity whose CRUD is the entire purpose of the .NET API.

**GS Requirement:** Requisitos técnicos → Persistência relacional + Relacionamento 1:N (lado "N").

**Complexity:** S

**Blocks:** N-07, N-09
**Requires:** N-02
**Can parallelize with:** N-05, N-12..N-16

### READ BEFORE STARTING
- `CONTEXT.md` → ".NET Entidades" (`AlertaHistorico` shape).
- OPUS prompt → "ENDPOINT CONTRACTS → AlertaController" (field names + nivelAlerta values: ATENCAO | ALERTA | EMERGENCIA).

### IMPLEMENT
**Create:** `Models/Entities/AlertaHistorico.cs`

**Full implementation spec:**
```csharp
public class AlertaHistorico
{
    public int Id { get; set; }
    public int ZonaId { get; set; }                 // FK → ZonaReferencia.Id
    public string NivelAlerta { get; set; } = string.Empty;  // ATENCAO|ALERTA|EMERGENCIA
    public double ScoreRegistrado { get; set; }     // 0.0–100.0
    public double No2Registrado { get; set; }       // ppb
    public string TextoRecomendacao { get; set; } = string.Empty; // max 1000
    public DateTime DtAlerta { get; set; } = DateTime.UtcNow;
    public bool Confirmado { get; set; } = false;
    public ZonaReferencia Zona { get; set; } = null!; // navigation
}
```
- `NivelAlerta` stays `string` at the entity level (Oracle stores VARCHAR2(15)); the **allowed values** are enforced by DTO validation (N-12), not a C# enum, to avoid EF enum-to-Oracle mapping friction. ASSUMPTION documented.
- **What NOT to do:** no score calculation, no satellite fields beyond the snapshot (`No2Registrado` is a *recorded* value, not a live read).

### TEST
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** compiles and `Zona`/`ZonaId` line up with `ZonaReferencia` for the FK config in N-07.

### COMMIT
```bash
git add Models/Entities/AlertaHistorico.cs
git commit -m "🗄️ feat(entity): add AlertaHistorico (N-side with FK to Zona) (task N-06)"
```

---

## N-07 · 1:N relationship configuration

**Objective:** Configure the `ZonaReferencia (1) → AlertaHistorico (N)` relationship with `Restrict` delete — the exact thing the GS jury asks ("o que acontece ao deletar um registro relacionado?").

**GS Requirement:** Requisitos técnicos → Relacionamento 1:N (configuration).

**Complexity:** S

**Blocks:** N-08, N-09
**Requires:** N-05, N-06
**Can parallelize with:** —

### READ BEFORE STARTING
- `CONTEXT.md` → ".NET DbContext com Oracle" → the `HasOne/WithMany/HasForeignKey/OnDelete(Restrict)` block.
- Both entity files from N-05/N-06.

### IMPLEMENT
**Create:** `Data/EntityConfigurations/AlertaHistoricoConfiguration.cs` implementing `IEntityTypeConfiguration<AlertaHistorico>` (keeps `OnModelCreating` thin and testable).

**Full implementation spec:**
```csharp
public class AlertaHistoricoConfiguration : IEntityTypeConfiguration<AlertaHistorico>
{
    public void Configure(EntityTypeBuilder<AlertaHistorico> e)
    {
        e.HasKey(a => a.Id);
        e.HasOne(a => a.Zona)
         .WithMany(z => z.Alertas)
         .HasForeignKey(a => a.ZonaId)
         .OnDelete(DeleteBehavior.Restrict); // deleting a Zona with alerts → blocked
        // Oracle column/sequence mappings are added in N-08 (same file)
    }
}
```
- `DeleteBehavior.Restrict`: deleting a `ZonaReferencia` that still has alerts throws `DbUpdateException` (→ 409 via N-19). This is the demonstrable answer for the presentation.
- **What NOT to do:** do not use `Cascade` (CONTEXT.md says "não cascade").

### TEST
Relationship is verified by the migration in N-10 and the smoke test in N-32.
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** compiles; FK config is in a dedicated `IEntityTypeConfiguration`.

### COMMIT
```bash
git add Data/EntityConfigurations/AlertaHistoricoConfiguration.cs
git commit -m "🗄️ feat(model): configure 1:N ZonaReferencia→AlertaHistorico (Restrict) (task N-07)"
```

---

## N-08 · Oracle-specific mappings (tables, columns, sequences, types)

**Objective:** Make EF Core emit valid Oracle DDL — the difference between a migration that applies and one that fails (a failing migration is a failing task per the prompt).

**GS Requirement:** Requisitos técnicos → Migration correta (must produce valid Oracle SQL).

**Complexity:** M

**Blocks:** N-09
**Requires:** N-07
**Can parallelize with:** —

### READ BEFORE STARTING
- OPUS prompt → "ORACLE + EF CORE SPECIFICS" (HiLo sequences, VARCHAR2 max length, NUMBER(1) for bool, DATE for DateTime).
- `CONTEXT.md` → ".NET DbContext" (table names `ALERTA_HISTORICO`, `ZONA_REFERENCIA_NET`).

### IMPLEMENT
**Create:** `Data/EntityConfigurations/ZonaReferenciaConfiguration.cs`
**Modify:** `Data/EntityConfigurations/AlertaHistoricoConfiguration.cs` (add column/type/sequence mappings)

**Full implementation spec — AlertaHistorico mappings:**
```csharp
e.ToTable("ALERTA_HISTORICO");
e.Property(a => a.Id).HasColumnName("ID_ALERTA").UseHiLo("SEQ_ALERTA_HISTORICO");
e.Property(a => a.ZonaId).HasColumnName("ID_ZONA").IsRequired();
e.Property(a => a.NivelAlerta).HasColumnName("NIVEL_ALERTA").HasMaxLength(15).IsRequired();
e.Property(a => a.ScoreRegistrado).HasColumnName("SCORE_REGISTRADO").HasColumnType("NUMBER(5,2)");
e.Property(a => a.No2Registrado).HasColumnName("NO2_REGISTRADO").HasColumnType("NUMBER(8,4)");
e.Property(a => a.TextoRecomendacao).HasColumnName("TEXTO_RECOMENDACAO").HasMaxLength(1000);
e.Property(a => a.DtAlerta).HasColumnName("DT_ALERTA").HasColumnType("DATE");
e.Property(a => a.Confirmado).HasColumnName("CONFIRMADO").HasColumnType("NUMBER(1)");
e.HasIndex(a => new { a.ZonaId, a.DtAlerta }).HasDatabaseName("IX_ALERTA_ZONA_DT"); // for stats queries (N-21)
```
**ZonaReferencia mappings:**
```csharp
e.ToTable("ZONA_REFERENCIA_NET");
e.Property(z => z.Id).HasColumnName("ID_ZONA").UseHiLo("SEQ_ZONA_REFERENCIA");
e.Property(z => z.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
e.Property(z => z.Municipio).HasColumnName("MUNICIPIO").HasMaxLength(100).IsRequired();
```
- `bool Confirmado` → `NUMBER(1)`: Oracle EF provider maps bool↔NUMBER(1) automatically, but the explicit `HasColumnType` makes the DDL unambiguous.
- `UseHiLo` (not IDENTITY): Oracle has no IDENTITY by default; HiLo creates `SEQ_*` and avoids round-trips.
- **What NOT to do:** do not rely on naming conventions; every column is mapped explicitly. Do not add a `Confirmado` check constraint via raw SQL here (keep migration provider-generated).

### MIGRATE
No migration yet — N-10 generates the first one *after* the DbContext (N-09) consumes these configs.

### TEST
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** both configuration classes compile and cover every property with explicit Oracle column names/types.

### COMMIT
```bash
git add Data/EntityConfigurations/
git commit -m "🗄️ feat(model): Oracle column mappings, HiLo sequences, index (task N-08)"
```

---

## N-09 · AppDbContext + DI wiring

**Objective:** Assemble the DbContext that applies both entity configurations and is injectable everywhere.

**GS Requirement:** Requisitos técnicos → Persistência relacional (foundation).

**Complexity:** M

**Blocks:** N-10, N-20, N-21, N-25
**Requires:** N-08
**Can parallelize with:** —

### READ BEFORE STARTING
- `CONTEXT.md` → ".NET DbContext com Oracle".
- `Program.cs` (N-04) — find the commented `AddDbContext` placeholder.

### IMPLEMENT
**Create:** `Data/AppDbContext.cs`
**Modify:** `Program.cs` — register the context with an env-composed connection string.

**Full implementation spec:**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AlertaHistorico> AlertasHistorico => Set<AlertaHistorico>();
    public DbSet<ZonaReferencia> ZonasReferencia => Set<ZonaReferencia>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfiguration(new ZonaReferenciaConfiguration());
        mb.ApplyConfiguration(new AlertaHistoricoConfiguration());
    }
}
```
**Program.cs registration (helper builds the string from env vars):**
```csharp
string oracleConn =
    $"User Id={Environment.GetEnvironmentVariable("DB_USER") ?? "system"};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASS") ?? "oracle"};" +
    $"Data Source={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"}:" +
    $"{Environment.GetEnvironmentVariable("DB_PORT") ?? "1521"}/" +
    $"{Environment.GetEnvironmentVariable("DB_SERVICE") ?? "XEPDB1"};";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseOracle(oracleConn));
```
- ASSUMPTION: connection string composed from discrete env vars (not a single `ConnectionStrings:Oracle` secret) so docker-compose's existing `DB_USER`/`DB_PASS` map cleanly and no secret is ever committed. The `appsettings.json` key from N-04 stays as documentation only.
- **What NOT to do:** no hardcoded password (the `?? "oracle"` default is dev-only and matches the compose default `${DB_PASS:-oracle}`); never log the connection string.

### MIGRATE
N-10 owns the first migration.

### TEST
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** build is clean and `AppDbContext` resolves both `IEntityTypeConfiguration`s. (Connectivity is proven in N-10/N-25.)

### COMMIT
```bash
git add Data/AppDbContext.cs Program.cs
git commit -m "🗄️ feat(data): AppDbContext + env-based Oracle DI registration (task N-09)"
```

---

## N-10 · InitialCreate migration

**Objective:** Produce the first EF Core migration and apply it against Oracle — the centerpiece of the discipline.

**GS Requirement:** Requisitos técnicos → Migration correta (the headline criterion).

**Complexity:** M

**Blocks:** N-11
**Requires:** N-09
**Can parallelize with:** —

### READ BEFORE STARTING
- `Data/AppDbContext.cs`, both configuration files.
- A reachable Oracle (local `gvenzl/oracle-xe` container from CONTEXT.md compose, or Clayton's). Confirm env vars are exported.

### IMPLEMENT
**Create:** `Data/Migrations/*_InitialCreate.cs` (generated).
**Modify:** `Program.cs` — enable the `Database.Migrate()` call inside the `IsDevelopment()` block (uncomment from N-04).

### MIGRATE
```bash
export DB_USER=system DB_PASS=oracle DB_HOST=localhost DB_PORT=1521 DB_SERVICE=XEPDB1
dotnet ef migrations add InitialCreate \
  --project PulsoUrbano.Net --output-dir Data/Migrations
dotnet ef database update --project PulsoUrbano.Net
dotnet ef migrations list --project PulsoUrbano.Net
```
**Expected last line:** `InitialCreate (Applied)`
- Inspect the generated `Up()`: must contain `CREATE TABLE "ALERTA_HISTORICO"` and `"ZONA_REFERENCIA_NET"`, two `CREATE SEQUENCE` (HiLo), the FK with `ON DELETE` *not* cascading, and the `IX_ALERTA_ZONA_DT` index.
- If it fails: do **not** delete the Migrations folder. Fix the entity mapping (most common: missing `HasColumnType` causing an invalid Oracle type) and `dotnet ef migrations remove` the bad one before regenerating.

### TEST
```bash
# Smoke: app boots and applies migration in Development
dotnet run --project PulsoUrbano.Net &
sleep 8 && curl -s http://localhost:5000/api/health   # health added in N-25; here just confirm boot
```
**Task is complete only when:** `dotnet ef migrations list` shows `InitialCreate (Applied)` and the two tables exist in Oracle (`SELECT table_name FROM user_tables WHERE table_name LIKE '%ALERTA%' OR table_name LIKE '%ZONA_REFERENCIA%';`).

### COMMIT
```bash
git add Data/Migrations/ Program.cs
git commit -m "🗃️ feat(migration): InitialCreate applied against Oracle (task N-10)"
```

---

## N-11 · DataSeeder (demo data) + migration verification

**Objective:** Seed 5 zonas + 40+ alertas so Swagger demos, statistics endpoints, and the video pitch show real, non-trivial data.

**GS Requirement:** Viabilidade e Inovação (10pts) — the seeded history is what makes "qual zona teve mais emergências e a tendência está melhorando?" demonstrable.

**Complexity:** M

**Blocks:** N-29 (tests want seeded data), N-32
**Requires:** N-10
**Can parallelize with:** —

### READ BEFORE STARTING
- `AppDbContext.cs`, both entities.
- OPUS prompt → "DataSeeder: 5 ZonaReferencia + 40+ AlertaHistorico for demo".

### IMPLEMENT
**Create:** `Data/DataSeeder.cs`
**Modify:** `Program.cs` — call `DataSeeder.SeedAsync(db)` after `Database.Migrate()` (Development only, idempotent).

**Full implementation spec:**
- `public static async Task SeedAsync(AppDbContext db)`:
  - If `db.ZonasReferencia.Any()` → return (idempotent).
  - Insert 5 zonas: `Centro`, `Zona Leste`, `Zona Sul`, `Zona Norte`, `Zona Oeste` (all `Municipio = "São Paulo"`).
  - Insert 40–50 `AlertaHistorico` spread across the 5 zonas over the last 30 days: vary `NivelAlerta` (ATENCAO/ALERTA/EMERGENCIA), `ScoreRegistrado` (20–80), `No2Registrado` (20–55), `DtAlerta` (now minus 0–30 days), `Confirmado` (mix true/false). Make `Centro` have the most EMERGENCIA rows so the "zonaComMaisAlertas" stat is non-trivial.
  - Use a fixed `Random(562999)` seed so demo data is reproducible.
- **What NOT to do:** no `INSERT ... VALUES (1,'test')`-style junk (CONTEXT.md / Clayton's rule); data must look realistic. Do not seed in Production.

### MIGRATE
No schema change. Verify migration still applied:
```bash
dotnet ef migrations list --project PulsoUrbano.Net   # InitialCreate (Applied)
```

### TEST
**Test file:** `tests/PulsoUrbano.Net.Tests/Data/DataSeederTests.cs`
```csharp
[Fact]
public async Task SeedAsync_FreshDb_Inserts5ZonasAndAtLeast40Alertas()
{
    // Arrange: SQLite in-memory context (N-03 fixture)
    // Act: await DataSeeder.SeedAsync(ctx);
    // Assert: ctx.ZonasReferencia.Count() == 5
    //         ctx.AlertasHistorico.Count() >= 40
}
[Fact]
public async Task SeedAsync_CalledTwice_DoesNotDuplicate()
{
    // Act: seed twice
    // Assert: counts unchanged after second call
}
```
```bash
dotnet test --filter "FullyQualifiedName~DataSeederTests" --logger "console;verbosity=minimal"
```
**Expected:** `Passed! - Failed: 0, Passed: 2`

**Task is complete only when:** booting in Development populates Oracle with 5 zonas and ≥40 alertas, and re-running does not duplicate.

### COMMIT
```bash
git add Data/DataSeeder.cs Program.cs tests/
git commit -m "🗄️ feat(data): idempotent seeder (5 zonas + 40+ alertas) (task N-11)"
```

---

## N-12 · AlertaCreateDTO + validation

**Objective:** Define the request body for `POST /api/alertas` with validation that rejects bad input before it reaches the DB.

**GS Requirement:** Requisitos técnicos → boas práticas (DTOs, validação) + "como a aplicação trata entradas inválidas".

**Complexity:** S

**Blocks:** N-20, N-23
**Requires:** N-02
**Can parallelize with:** N-05, N-06, N-13..N-16

### READ BEFORE STARTING
- OPUS prompt → "ENDPOINT CONTRACTS → POST /api/alertas" (body shape, nivelAlerta values, score range, textoRecomendacao max 1000).

### IMPLEMENT
**Create:** `Models/DTOs/AlertaCreateDTO.cs` + `Validators/AlertaCreateDTOValidator.cs`

**Full implementation spec:**
```csharp
public record AlertaCreateDTO(
    int ZonaId,
    string NivelAlerta,
    double ScoreRegistrado,
    double No2Registrado,
    string TextoRecomendacao);
```
FluentValidation rules:
- `ZonaId` > 0.
- `NivelAlerta` ∈ {`ATENCAO`,`ALERTA`,`EMERGENCIA`} (case-sensitive, `.Must`).
- `ScoreRegistrado` between 0.0 and 100.0.
- `No2Registrado` ≥ 0.
- `TextoRecomendacao` not empty, max length 1000.
- ASSUMPTION: `record` (immutable) for all DTOs (CONTEXT.md uses records on the Java side; mirror the pattern).
- **What NOT to do:** do not expose the entity; no `Id`, `DtAlerta`, or `Confirmado` in the create DTO (server-controlled).

### TEST
**Test file:** `tests/.../DTOs/AlertaCreateDTOValidatorTests.cs`
```csharp
[Theory] // valid + each invalid case
public void Validate_NivelAlertaInvalid_Fails() { /* "FOO" → invalid */ }
[Fact]
public void Validate_ScoreOver100_Fails() { }
[Fact]
public void Validate_ValidPayload_Passes() { }
```
```bash
dotnet test --filter "FullyQualifiedName~AlertaCreateDTOValidatorTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** validator rejects out-of-range score, invalid nivel, empty texto, and >1000-char texto; accepts a valid payload.

### COMMIT
```bash
git add Models/DTOs/AlertaCreateDTO.cs Validators/AlertaCreateDTOValidator.cs tests/
git commit -m "🏷️ feat(dto): AlertaCreateDTO + FluentValidation rules (task N-12)"
```

---

## N-13 · AlertaResponseDTO

**Objective:** Define the response shape returned by every Alerta endpoint, including the embedded `zonaNome`, so no entity is ever serialized.

**GS Requirement:** Requisitos técnicos → boas práticas (never expose entity).

**Complexity:** S

**Blocks:** N-20, N-23
**Requires:** N-02
**Can parallelize with:** N-12, N-14..N-16

### READ BEFORE STARTING
- OPUS prompt → "POST /api/alertas Response 201" (exact field list).

### IMPLEMENT
**Create:** `Models/DTOs/AlertaResponseDTO.cs`
```csharp
public record AlertaResponseDTO(
    int Id, int ZonaId, string ZonaNome, string NivelAlerta,
    double ScoreRegistrado, double No2Registrado, string TextoRecomendacao,
    DateTime DtAlerta, bool Confirmado);
```
- Add a static `FromEntity(AlertaHistorico a)` mapper (or do it in the service); `ZonaNome` from `a.Zona?.Nome ?? ""`.
- **What NOT to do:** no navigation properties, no lazy-loaded collections.

### TEST
Covered indirectly by controller tests (N-30). Build-only here.
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
```
**Task is complete only when:** the record matches the contract field-for-field (names + types).

### COMMIT
```bash
git add Models/DTOs/AlertaResponseDTO.cs
git commit -m "🏷️ feat(dto): AlertaResponseDTO with embedded zonaNome (task N-13)"
```

---

## N-14 · AlertaConfirmarDTO

**Objective:** Tiny request DTO for `PUT /api/alertas/{id}/confirmar`.

**GS Requirement:** Requisitos técnicos → boas práticas.

**Complexity:** S

**Blocks:** N-23
**Requires:** N-02
**Can parallelize with:** N-12, N-13, N-15, N-16

### IMPLEMENT
**Create:** `Models/DTOs/AlertaConfirmarDTO.cs`
```csharp
public record AlertaConfirmarDTO(bool Confirmado);
```
- **What NOT to do:** don't reuse `AlertaCreateDTO` for the partial update.

### TEST
Build-only.
**Task is complete only when:** compiles.

### COMMIT
```bash
git add Models/DTOs/AlertaConfirmarDTO.cs
git commit -m "🏷️ feat(dto): AlertaConfirmarDTO (task N-14)"
```

---

## N-15 · Statistics DTOs (Zona + Resumo) + ZonaReferenciaDTO

**Objective:** Define the typed response shapes for the two statistics endpoints so Swagger shows real schemas (no anonymous objects — a quality gate).

**GS Requirement:** Viabilidade e Inovação (10pts) — these DTOs *are* the insight surface; Requisitos técnicos (typed responses).

**Complexity:** S

**Blocks:** N-21, N-24
**Requires:** N-02
**Can parallelize with:** N-12, N-13, N-14, N-16

### READ BEFORE STARTING
- OPUS prompt → "EstatisticasController" (both response shapes, incl. `alertasPorNivel`, `tendencia`, `zonaComMaisAlertas`).

### IMPLEMENT
**Create:** `Models/DTOs/EstatisticasZonaDTO.cs`, `Models/DTOs/EstatisticasResumoDTO.cs`, `Models/DTOs/ZonaReferenciaDTO.cs`
```csharp
public record PeriodoDTO(DateTime Inicio, DateTime Fim);

public record EstatisticasZonaDTO(
    int ZonaId, string ZonaNome, PeriodoDTO Periodo,
    int TotalAlertas, Dictionary<string,int> AlertasPorNivel,
    double ScoreMinimo, double ScoreMaximo, double ScoreMedia,
    int DiasComAlerta, DateTime? PiorDia, string Tendencia); // MELHORANDO|PIORANDO|ESTAVEL

public record ZonaResumoDTO(int Id, string Nome, int Total);

public record EstatisticasResumoDTO(
    int TotalZonas, int TotalAlertas30dias, ZonaResumoDTO ZonaComMaisAlertas,
    string NivelPredominante, DateTime DtAtualizacao);

public record ZonaReferenciaDTO(int Id, string Nome, string Municipio);
```
- ASSUMPTION: `AlertasPorNivel` as `Dictionary<string,int>` keyed by nivel — matches the JSON object in the contract.
- **What NOT to do:** don't compute anything here (DTOs are dumb); logic is in N-21.

### TEST
Build-only; values verified in N-32.
**Task is complete only when:** all records compile and match the contract field names.

### COMMIT
```bash
git add Models/DTOs/Estatisticas*.cs Models/DTOs/ZonaReferenciaDTO.cs
git commit -m "🏷️ feat(dto): statistics + zona DTOs (typed Swagger schemas) (task N-15)"
```

---

## N-16 · ErrorResponseDTO + PaginatedResponseDTO<T>

**Objective:** Standard error envelope and pagination wrapper used by the exception middleware and the list endpoint.

**GS Requirement:** Requisitos técnicos → boas práticas (padronização de respostas, paginação).

**Complexity:** S

**Blocks:** N-19, N-20, N-23
**Requires:** N-02
**Can parallelize with:** N-12..N-15

### READ BEFORE STARTING
- OPUS prompt → error envelope `{ status, erro, ... }` and `GET /api/alertas` paginated shape `{ total, pagina, tamanhoPagina, dados }`.

### IMPLEMENT
**Create:** `Models/DTOs/ErrorResponseDTO.cs`, `Models/DTOs/PaginatedResponseDTO.cs`
```csharp
public record ErrorResponseDTO(int Status, string Erro, string? Mensagem = null,
                               IEnumerable<string>? Campos = null);
public record PaginatedResponseDTO<T>(int Total, int Pagina, int TamanhoPagina,
                                      IReadOnlyList<T> Dados);
```
- **What NOT to do:** don't leak stack traces into `Mensagem` (N-19 sets a safe message).

### TEST
Build-only; exercised in N-30/N-31.
**Task is complete only when:** compiles and matches the contract keys (`status`, `erro`, `campos`, `total`, `pagina`, `tamanhoPagina`, `dados`).

### COMMIT
```bash
git add Models/DTOs/ErrorResponseDTO.cs Models/DTOs/PaginatedResponseDTO.cs
git commit -m "🏷️ feat(dto): ErrorResponseDTO + PaginatedResponseDTO<T> (task N-16)"
```

---

## N-17 · JwtValidationMiddleware

**Objective:** Validate the Bearer token issued by the Java API using the shared `JWT_SECRET`, with zero HTTP calls to Java.

**GS Requirement:** Modelagem avançada/segurança equivalent (JWT validation) + Requisitos técnicos (boas práticas de segurança). Maps to GS .NET "boas práticas".

**Complexity:** M

**Blocks:** N-18, protected-endpoint tests (N-31)
**Requires:** N-04
**Can parallelize with:** N-19

### READ BEFORE STARTING
- OPUS prompt → "JWT VALIDATION (N-17 to N-18)" + the middleware order block (must run after CORS, before routing).
- `CONTEXT.md` → docker-compose: both APIs share `JWT_SECRET=${JWT_SECRET:-pulso-secret-2026}`. Java `jwt.secret` default differs (`pulso-urbano-secret-key-2026-gs-fiap`) — see ASSUMPTION/Q-01.

### IMPLEMENT
**Create:** `Middleware/JwtValidationMiddleware.cs`
**Modify:** `Program.cs` — enable `app.UseMiddleware<JwtValidationMiddleware>();` at pipeline step 5.

**Full implementation spec:**
- Reads `Authorization: Bearer {token}`.
- Validates signature with `JWT_SECRET` (HMAC-SHA256) via `JwtSecurityTokenHandler` + `TokenValidationParameters` (`ValidateIssuerSigningKey=true`, `ValidateLifetime=true`, `ValidateIssuer`/`ValidateAudience` from config — see Q-01).
- On success: attach claims (`usuarioId`, `email`, `role`) to `HttpContext.Items["JwtClaims"]` and `await _next(ctx)`.
- On missing/invalid token for a **protected** route: short-circuit with `401` + `ErrorResponseDTO(401,"Token ausente ou inválido")` (JSON).
- Public-route bypass list is applied in N-18 (this task validates; N-18 decides *when* validation is required).
- Use `ILogger<JwtValidationMiddleware>` for warnings (no `Console.WriteLine`).
- ASSUMPTION: HMAC symmetric key (Java uses JJWT HMAC by default). If Java switches to RSA, this becomes RSA public-key validation — flagged in Q-01.

### TEST
**Test file:** `tests/.../Middleware/JwtValidationMiddlewareTests.cs`
```csharp
[Fact] public async Task NoToken_ProtectedRoute_Returns401() { }
[Fact] public async Task ValidToken_PassesThrough_SetsClaims() { /* sign a token with the same secret */ }
[Fact] public async Task ExpiredToken_Returns401() { }
```
```bash
dotnet test --filter "FullyQualifiedName~JwtValidationMiddlewareTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** a token signed with the shared secret passes and exposes claims; a missing/expired/tampered token yields 401 with the error envelope.

### COMMIT
```bash
git add Middleware/JwtValidationMiddleware.cs Program.cs tests/
git commit -m "🔒 feat(security): JWT validation middleware (shared secret, no Java calls) (task N-17)"
```

---

## N-18 · Public-route bypass + role (403) rules

**Objective:** Decide which routes require a token and which are public, and return 403 when a valid token lacks the needed role.

**GS Requirement:** Requisitos técnicos → boas práticas (authorization).

**Complexity:** S

**Blocks:** protected-endpoint tests (N-31)
**Requires:** N-17
**Can parallelize with:** —

### READ BEFORE STARTING
- OPUS prompt → "Public endpoints bypass: GET /api/health, GET /swagger/*, GET /api-docs/*" and the per-endpoint auth column in ENDPOINT CONTRACTS (POST/PUT/DELETE alertas = protected; all GETs + estatisticas + health = public).

### IMPLEMENT
**Modify:** `Middleware/JwtValidationMiddleware.cs`

**Full implementation spec — bypass predicate:**
- Public (skip validation entirely): `GET /api/health`, any path starting `/swagger`, `/api-docs`, plus **any `GET`** on `/api/alertas`, `/api/alertas/{id}`, `/api/estatisticas/*` (reads are public per contract).
- Protected: `POST /api/alertas`, `PUT /api/alertas/{id}/confirmar`, `DELETE /api/alertas/{id}`.
- Role rule: for now all authenticated users may write (no admin-only alerta op in the contract). Implement the 403 path generically — `if (requiredRole != null && !claims.role.Contains(requiredRole)) → 403 ErrorResponseDTO` — but no endpoint sets `requiredRole` yet. ASSUMPTION documented; keeps the hook ready without inventing scope.
- **What NOT to do:** do not gate GETs behind auth (contract says reads are public — the mobile app reads without login on some screens).

### TEST
```csharp
[Fact] public async Task GetAlertas_NoToken_PassesThrough() { } // public
[Fact] public async Task PostAlertas_NoToken_Returns401() { }   // protected
[Fact] public async Task Health_NoToken_PassesThrough() { }
```
```bash
dotnet test --filter "FullyQualifiedName~JwtValidationMiddlewareTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** GET/health/swagger bypass; POST/PUT/DELETE alertas require a valid token.

### COMMIT
```bash
git add Middleware/JwtValidationMiddleware.cs tests/
git commit -m "🔒 feat(security): public-route bypass + role 403 hook (task N-18)"
```

---

## N-19 · GlobalExceptionMiddleware

**Objective:** Catch all unhandled exceptions and translate them into the standard `ErrorResponseDTO` with correct status codes — the answer to "como a aplicação trata erros".

**GS Requirement:** Requisitos técnicos → boas práticas (tratamento de exceções, respostas padronizadas).

**Complexity:** M

**Blocks:** integration tests rely on it (N-30..N-32)
**Requires:** N-04, N-16
**Can parallelize with:** N-17

### READ BEFORE STARTING
- OPUS prompt → "EXCEPTION HANDLING (N-19)" exception→status mapping.
- It must be the FIRST middleware (pipeline step 1).

### IMPLEMENT
**Create:** `Exceptions/GlobalExceptionMiddleware.cs`
**Modify:** `Program.cs` — `app.UseMiddleware<GlobalExceptionMiddleware>();` as the first `Use*` call.

**Full implementation spec — exception map:**
- `KeyNotFoundException` → 404 `ErrorResponseDTO(404,"Não encontrado", ex.Message)`
- `FluentValidation.ValidationException` → 400 with `Campos` = the failure messages
- `DbUpdateException` → 409 `ErrorResponseDTO(409,"Conflito de dados")` (covers the Restrict-delete case from N-07)
- `UnauthorizedAccessException` → 401
- fallback `Exception` → 500 `ErrorResponseDTO(500,"Erro interno")`
- Log with `ILogger<GlobalExceptionMiddleware>` at `Error` level **including** stack trace server-side, but never put the stack trace in the response body (prompt rule).
- Sets `ContentType = application/json` and serializes with the app's JSON options.

### TEST
**Test file:** `tests/.../Exceptions/GlobalExceptionMiddlewareTests.cs`
```csharp
[Fact] public async Task KeyNotFound_Maps404() { }
[Fact] public async Task ValidationException_Maps400WithCampos() { }
[Fact] public async Task Generic_Maps500_NoStackTraceInBody() { }
```
```bash
dotnet test --filter "FullyQualifiedName~GlobalExceptionMiddlewareTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** each exception type yields the right status + envelope and no stack trace leaks into the response.

### COMMIT
```bash
git add Exceptions/GlobalExceptionMiddleware.cs Program.cs tests/
git commit -m "🛡️ feat(error): global exception middleware → ErrorResponseDTO (task N-19)"
```

---

## N-20 · IAlertaService + AlertaService

**Objective:** Encapsulate all Alerta business logic and EF access so controllers stay thin.

**GS Requirement:** Requisitos técnicos → boas práticas (organização em camadas / separação de responsabilidades).

**Complexity:** L

**Blocks:** N-22, N-23
**Requires:** N-09, N-12, N-13, N-14, N-16
**Can parallelize with:** N-21

### READ BEFORE STARTING
- `AppDbContext.cs`; all Alerta DTOs (N-12/13/14/16).
- OPUS prompt → AlertaController contract (paging defaults, soft vs hard delete decision).

### IMPLEMENT
**Create:** `Services/IAlertaService.cs`, `Services/AlertaService.cs`

**Full implementation spec — method signatures:**
```csharp
Task<AlertaResponseDTO> CreateAsync(AlertaCreateDTO dto);
Task<AlertaResponseDTO> GetByIdAsync(int id);              // throws KeyNotFoundException → 404
Task<PaginatedResponseDTO<AlertaResponseDTO>> GetAsync(
        int? zonaId, int dias = 30, int pagina = 1, int tamanhoPagina = 20);
Task<AlertaResponseDTO> ConfirmarAsync(int id, bool confirmado);
Task DeleteAsync(int id);
```
- `CreateAsync`: validate `ZonaId` exists (else `KeyNotFoundException`); set `DtAlerta = DateTime.UtcNow`, `Confirmado = false`; save; reload with `Zona` to fill `ZonaNome`.
- `GetAsync`: filter by `zonaId` (if provided) and `DtAlerta >= UtcNow.AddDays(-dias)`; `.Include(a => a.Zona)`; `.AsNoTracking()`; order by `DtAlerta desc`; total count + skip/take pagination.
- `ConfirmarAsync`: load tracked, set `Confirmado`, save, return DTO.
- `DeleteAsync`: **hard delete** (`Remove` + save). DECISION/ASSUMPTION: hard delete — there is no `Ativo` flag in the entity and the contract returns `204`; soft delete would need a schema change the contract doesn't specify. Documented.
- All reads use `.AsNoTracking()` (quality gate).
- **What NOT to do:** no controller logic here; no `Console.WriteLine`; never return the entity.

### MIGRATE
None (no entity change).

### TEST
**Test file:** `tests/.../Services/AlertaServiceTests.cs` (SQLite in-memory, seeded via N-11 seeder)
```csharp
[Fact] public async Task CreateAsync_ValidDto_PersistsAndReturnsDtoWithZonaNome() { }
[Fact] public async Task GetByIdAsync_UnknownId_ThrowsKeyNotFound() { }
[Fact] public async Task GetAsync_FiltersByZonaAndDias_AndPaginates() { }
[Fact] public async Task ConfirmarAsync_SetsConfirmadoTrue() { }
[Fact] public async Task DeleteAsync_RemovesRecord_SubsequentGetThrows() { }
```
```bash
dotnet test --filter "FullyQualifiedName~AlertaServiceTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** all five behaviors pass against SQLite in-memory.

### COMMIT
```bash
git add Services/IAlertaService.cs Services/AlertaService.cs tests/
git commit -m "✨ feat(service): AlertaService CRUD with paging + AsNoTracking (task N-20)"
```

---

## N-21 · IEstatisticasService + EstatisticasService

**Objective:** Compute the aggregated insights (per zona and global resumo) that justify "Viabilidade e Inovação".

**GS Requirement:** Viabilidade e Inovação (10pts) — primary; Requisitos técnicos (boas práticas).

**Complexity:** L

**Blocks:** N-22, N-24
**Requires:** N-09, N-15
**Can parallelize with:** N-20

### READ BEFORE STARTING
- OPUS prompt → EstatisticasController contract (both response shapes, `tendencia` logic, `zonaComMaisAlertas`).
- The `IX_ALERTA_ZONA_DT` index from N-08 (queries should use ZonaId+DtAlerta).

### IMPLEMENT
**Create:** `Services/IEstatisticasService.cs`, `Services/EstatisticasService.cs`
```csharp
Task<EstatisticasZonaDTO> GetByZonaAsync(int zonaId, int dias = 30);  // 404 if zona unknown
Task<EstatisticasResumoDTO> GetResumoGeralAsync();
```
- `GetByZonaAsync`: window = last `dias`; `TotalAlertas`, `AlertasPorNivel` (group by `NivelAlerta`), `ScoreMinimo/Maximo/Media`, `DiasComAlerta` (distinct `DtAlerta.Date`), `PiorDia` (date of min score).
  - `Tendencia`: split the window into first half vs second half, compare average score. If second-half avg > first-half avg + 2 → `MELHORANDO`; if < first-half avg − 2 → `PIORANDO`; else `ESTAVEL`. ASSUMPTION: ±2-point threshold (documented; demonstrable and stable for the seeded data).
- `GetResumoGeralAsync`: `TotalZonas` = count of zonas; `TotalAlertas30dias`; `ZonaComMaisAlertas` (top zona by 30-day alert count → `ZonaResumoDTO`); `NivelPredominante` (mode of `NivelAlerta` in 30 days); `DtAtualizacao = UtcNow`.
- All queries `.AsNoTracking()`; prefer DB-side grouping where the Oracle provider supports it, otherwise project the minimal columns and aggregate in memory (document which, to avoid silent client-eval).
- **What NOT to do:** no recommendation text generation, no score recomputation (Java owns scoring; .NET only aggregates recorded values).

### TEST
**Test file:** `tests/.../Services/EstatisticasServiceTests.cs`
```csharp
[Fact] public async Task GetByZonaAsync_KnownZona_ReturnsCorrectCountsAndAvg() { }
[Fact] public async Task GetByZonaAsync_UnknownZona_ThrowsKeyNotFound() { }
[Fact] public async Task GetByZonaAsync_Tendencia_ImprovingWindow_ReturnsMELHORANDO() { }
[Fact] public async Task GetResumoGeralAsync_PicksZonaWithMostAlertas() { } // Centro per seeder
```
```bash
dotnet test --filter "FullyQualifiedName~EstatisticasServiceTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** aggregates match a hand-computed fixture and `tendencia` is correct for a controlled dataset.

### COMMIT
```bash
git add Services/IEstatisticasService.cs Services/EstatisticasService.cs tests/
git commit -m "📊 feat(service): aggregated statistics per zona + resumo geral (task N-21)"
```

---

## N-22 · Service + validation DI registration

**Objective:** Register services and FluentValidation in the DI container so controllers resolve them.

**GS Requirement:** Requisitos técnicos → boas práticas (injeção de dependência).

**Complexity:** S

**Blocks:** N-23, N-24
**Requires:** N-20, N-21
**Can parallelize with:** —

### READ BEFORE STARTING
- `Program.cs` (N-04/N-09).
- Service interfaces from N-20/N-21; validators from N-12.

### IMPLEMENT
**Modify:** `Program.cs`
```csharp
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IEstatisticasService, EstatisticasService>();
builder.Services.AddValidatorsFromAssemblyContaining<AlertaCreateDTOValidator>();
builder.Services.AddFluentValidationAutoValidation(); // 400 on invalid model
```
- **What NOT to do:** no `AddSingleton` for DbContext-dependent services (must be `Scoped`).

### TEST
```bash
dotnet build PulsoUrbano.Net/PulsoUrbano.Net.csproj -q
dotnet run --project PulsoUrbano.Net &  # boots without DI resolution errors
```
**Task is complete only when:** the app starts and a controller (next tasks) can resolve `IAlertaService`/`IEstatisticasService` without "unable to resolve service" errors.

### COMMIT
```bash
git add Program.cs
git commit -m "💉 feat(di): register services + FluentValidation auto-validation (task N-22)"
```

---

## N-23 · AlertaController

**Objective:** Expose the full Alerta CRUD over HTTP exactly per the contract — the bulk of the "API REST boas práticas" points.

**GS Requirement:** Requisitos técnicos → API REST boas práticas + Persistência relacional + Relacionamento 1:N (via responses).

**Complexity:** L

**Blocks:** N-26, N-30, N-31
**Requires:** N-20, N-22, N-13, N-14, N-16
**Can parallelize with:** N-24

### READ BEFORE STARTING
- OPUS prompt → "ENDPOINT CONTRACTS → AlertaController" (every verb, status, header).
- `IAlertaService` (N-20).

### IMPLEMENT
**Create:** `Controllers/AlertaController.cs`

**Full implementation spec:**
- `[ApiController] [Route("api/alertas")]`, primary-constructor inject `IAlertaService`.
- `POST /` → `CreateAsync`; return `CreatedAtAction(nameof(GetById), new { id }, dto)` → **201 + `Location` header**.
- `GET /` → `GetAsync(zonaId?, dias=30, pagina=1, tamanhoPagina=20)` → 200 `PaginatedResponseDTO<AlertaResponseDTO>`.
- `GET /{id}` → 200 `AlertaResponseDTO` | 404 (KeyNotFound bubbles to N-19).
- `PUT /{id}/confirmar` → body `AlertaConfirmarDTO` → 200.
- `DELETE /{id}` → 204.
- `[ProducesResponseType]` on every action (status + type) — finalized/audited in N-26.
- **What NOT to do:** no business logic, no direct `DbContext` access, no entity in any signature.

### TEST
Covered by integration tests N-30/N-31. Build + manual curl here:
```bash
dotnet run --project PulsoUrbano.Net &
curl -s "http://localhost:5000/api/alertas?zonaId=1&dias=30" | head -c 300
```
**Task is complete only when:** `POST` returns 201 with a `Location` header and the created resource is retrievable via `GET /api/alertas/{id}`.

### COMMIT
```bash
git add Controllers/AlertaController.cs
git commit -m "✨ feat(api): AlertaController CRUD per contract (201/200/404/204) (task N-23)"
```

---

## N-24 · EstatisticasController

**Objective:** Expose the two statistics endpoints that demonstrate innovation/viability.

**GS Requirement:** Viabilidade e Inovação (10pts) + Requisitos técnicos (API REST).

**Complexity:** M

**Blocks:** N-26, N-32
**Requires:** N-21, N-22, N-15, N-16
**Can parallelize with:** N-23

### READ BEFORE STARTING
- OPUS prompt → "EstatisticasController" contract.
- `IEstatisticasService` (N-21).

### IMPLEMENT
**Create:** `Controllers/EstatisticasController.cs`
- `[ApiController] [Route("api/estatisticas")]`.
- `GET /zona/{zonaId}?dias=30` → 200 `EstatisticasZonaDTO` | 404.
- `GET /resumo` → 200 `EstatisticasResumoDTO`.
- Both public (no token). `[ProducesResponseType]` on each.
- **What NOT to do:** no LINQ/aggregation in the controller (delegate to service — quality gate).

### TEST
```bash
curl -s "http://localhost:5000/api/estatisticas/resumo" | head -c 300
curl -s "http://localhost:5000/api/estatisticas/zona/1?dias=30" | head -c 300
```
**Task is complete only when:** `/resumo` returns the global shape and `/zona/{id}` returns the per-zona shape with a valid `tendencia`.

### COMMIT
```bash
git add Controllers/EstatisticasController.cs
git commit -m "📊 feat(api): EstatisticasController (zona + resumo) (task N-24)"
```

---

## N-25 · HealthController

**Objective:** Provide `GET /api/health` for the Docker healthcheck and a fast "is the DB reachable?" check — required for Clayton's compose healthcheck.

**GS Requirement:** Apresentação (Docker demo) + Requisitos técnicos (boas práticas).

**Complexity:** S

**Blocks:** N-26
**Requires:** N-09
**Can parallelize with:** N-23, N-24

### READ BEFORE STARTING
- OPUS prompt → "HealthController" response shape.
- `AppDbContext` (for `CanConnectAsync`).

### IMPLEMENT
**Create:** `Controllers/HealthController.cs`
- `[ApiController] [Route("api/health")]`, public.
- `GET /` → `{ status, servico:"pulso-urbano-dotnet", versao:"1.0.0", timestamp, database }` where `database = await db.Database.CanConnectAsync() ? "connected" : "error: ..."`.
- Define a small `HealthResponseDTO` record (no anonymous object — quality gate).
- **What NOT to do:** don't run a heavy query; `CanConnectAsync` is enough.

### TEST
**Test file:** `tests/.../Controllers/HealthControllerTests.cs`
```csharp
[Fact] public async Task Get_ReturnsHealthy_WithConnectedDatabase() { } // SQLite connected
```
```bash
dotnet test --filter "FullyQualifiedName~HealthControllerTests" --logger "console;verbosity=minimal"
curl -s http://localhost:5000/api/health
```
**Task is complete only when:** `/api/health` returns 200 with `status:"healthy"` and `database:"connected"` against a reachable DB.

### COMMIT
```bash
git add Controllers/HealthController.cs tests/
git commit -m "✨ feat(api): HealthController for Docker healthcheck (task N-25)"
```

---

## N-26 · Controller conventions audit (ProducesResponseType + routing)

**Objective:** Sweep all three controllers for consistent attributes so Swagger (N-27/28) renders accurate status codes and schemas.

**GS Requirement:** Documentação Github + Apresentação (Swagger accuracy) + Requisitos técnicos.

**Complexity:** S

**Blocks:** N-27
**Requires:** N-23, N-24, N-25
**Can parallelize with:** —

### READ BEFORE STARTING
- All three controllers.
- Quality gates list: every action needs `[ProducesResponseType]`; no anonymous return objects.

### IMPLEMENT
**Modify:** `Controllers/*.cs`
- Add/verify `[ProducesResponseType(typeof(T), StatusCodes.Status2xx)]` and the error codes (400/401/404/409 as applicable) for **every** action.
- Confirm route prefixes: `api/alertas`, `api/estatisticas`, `api/health`.
- Confirm content type `Produces("application/json")` at controller level.
- **What NOT to do:** don't change behavior; this is attributes-only.

### TEST
```bash
dotnet build -q
dotnet run --project PulsoUrbano.Net &
curl -s http://localhost:5000/swagger/v1/swagger.json | python3 -c "import sys,json;d=json.load(sys.stdin);print(list(d['paths'].keys()))"
```
**Expected paths:** `/api/alertas`, `/api/alertas/{id}`, `/api/alertas/{id}/confirmar`, `/api/estatisticas/zona/{zonaId}`, `/api/estatisticas/resumo`, `/api/health`.

**Task is complete only when:** every action declares its response types and Swagger lists all six paths.

### COMMIT
```bash
git add Controllers/
git commit -m "📝 chore(api): ProducesResponseType + routing audit on all controllers (task N-26)"
```

---

## N-27 · SwaggerGen configuration (JWT scheme + XML comments)

**Objective:** Configure Swagger so the professor can authorize with a Bearer token and test protected endpoints live — the .NET "presentation" is Swagger.

**GS Requirement:** Documentação Github (10pts) + Apresentação (30pts).

**Complexity:** M

**Blocks:** N-28
**Requires:** N-26
**Can parallelize with:** —

### READ BEFORE STARTING
- `Program.cs` `AddSwaggerGen()` placeholder (N-04).
- `.csproj` `GenerateDocumentationFile` (N-01).

### IMPLEMENT
**Modify:** `Program.cs` `AddSwaggerGen(...)`
- `SwaggerDoc("v1", new OpenApiInfo { Title="Pulso Urbano .NET", Version="v1", Description="API secundária — histórico de alertas e estatísticas" })`.
- Add Bearer security definition + requirement so the "Authorize" button appears.
- `c.EnableAnnotations()` (Swashbuckle.Annotations).
- `c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PulsoUrbano.Net.xml"))`.
- **What NOT to do:** don't restrict Swagger to Development (CONTEXT.md/prompt: expose in all envs for the demo).

### TEST
```bash
dotnet run --project PulsoUrbano.Net &
# open http://localhost:5000/swagger — confirm "Authorize" button + all endpoints
curl -s http://localhost:5000/swagger/v1/swagger.json | grep -c "Bearer"
```
**Task is complete only when:** Swagger UI shows the "Authorize" (Bearer) button and all endpoints, served in every environment.

### COMMIT
```bash
git add Program.cs
git commit -m "📝 feat(docs): SwaggerGen with JWT Bearer scheme + XML comments (task N-27)"
```

---

## N-28 · Swagger annotations + example values on every action

**Objective:** Document each endpoint with summaries, response descriptions, and example payloads so the live demo reads cleanly.

**GS Requirement:** Documentação Github (10pts) + Apresentação (30pts).

**Complexity:** M

**Blocks:** N-35
**Requires:** N-27
**Can parallelize with:** —

### READ BEFORE STARTING
- All controllers; the contract examples in the OPUS prompt (use those exact example values).

### IMPLEMENT
**Modify:** `Controllers/*.cs` + DTO XML docs.
- Add XML `/// <summary>` + `/// <param>` + `/// <returns>` to every action (consumed by `IncludeXmlComments`).
- Add `[SwaggerResponse(201, "Alerta criado", typeof(AlertaResponseDTO))]` etc. for non-trivial responses.
- Provide example values matching the contract (e.g. `lat`/`lon` n/a here; use `zonaId:1, nivelAlerta:"ALERTA", scoreRegistrado:38.5`).
- Re-enable XML-comment warnings by removing `1591` from `NoWarn` once all public members are documented (optional but clean).
- **What NOT to do:** don't fabricate endpoints; document only what exists.

### TEST
```bash
dotnet build -q   # 0 warnings if 1591 re-enabled and all documented
dotnet run --project PulsoUrbano.Net &
# Swagger: every endpoint has a description + example
```
**Task is complete only when:** every endpoint in Swagger shows a summary and at least one example response; build is warning-clean.

### COMMIT
```bash
git add Controllers/ Models/DTOs/ PulsoUrbano.Net/PulsoUrbano.Net.csproj
git commit -m "📝 docs(swagger): summaries + example values on all endpoints (task N-28)"
```

---

## N-29 · WebApplicationFactory integration fixture

**Objective:** Build the test host that swaps Oracle for SQLite in-memory and seeds demo data, so integration tests hit the real pipeline (middlewares + controllers) without Oracle.

**GS Requirement:** Requisitos técnicos → boas práticas (testes de integração).

**Complexity:** L

**Blocks:** N-30, N-31, N-32
**Requires:** N-03, N-11
**Can parallelize with:** —

### READ BEFORE STARTING
- `Program.cs` (`public partial class Program`), `AppDbContext`, `DataSeeder`, `SqliteInMemoryFixture` (N-03).

### IMPLEMENT
**Create:** `tests/.../Infrastructure/PulsoWebAppFactory.cs : WebApplicationFactory<Program>`
- Override `ConfigureWebHost`: remove the registered `DbContextOptions<AppDbContext>` and re-add `UseSqlite` against an open in-memory connection; set `ASPNETCORE_ENVIRONMENT=Testing`.
- After build, in a scope: `db.Database.EnsureCreated()` then `DataSeeder.SeedAsync(db)`.
- Provide a helper to mint a valid JWT signed with the test `JWT_SECRET` for protected-route tests.
- ASSUMPTION: `Testing` environment skips the Oracle `Database.Migrate()` startup block (guard the migrate call with `!IsEnvironment("Testing")`). Add that guard to `Program.cs` in this task.

### TEST
Self-verifying via the tasks that consume it; add one smoke test:
```csharp
[Fact] public async Task Factory_Boots_HealthReturns200() {
    var client = _factory.CreateClient();
    var res = await client.GetAsync("/api/health");
    res.StatusCode.Should().Be(HttpStatusCode.OK);
}
```
```bash
dotnet test --filter "FullyQualifiedName~Factory" --logger "console;verbosity=minimal"
```
**Task is complete only when:** the factory boots the full app over SQLite, seeds data, and `/api/health` returns 200 in-process.

### COMMIT
```bash
git add tests/ Program.cs
git commit -m "🧪 test(infra): WebApplicationFactory over SQLite + seed (task N-29)"
```

---

## N-30 · Alerta CRUD happy-path + 404 integration tests

**Objective:** Prove POST→GET round-trips and unknown ids return 404 — the "como testaram as rotas" evidence.

**GS Requirement:** Requisitos técnicos → boas práticas.

**Complexity:** M

**Blocks:** —
**Requires:** N-29, N-23
**Can parallelize with:** N-31, N-32

### IMPLEMENT
**Create:** `tests/.../Integration/AlertaControllerTests.cs` (`[Trait("Category","Integration")]`)
```csharp
[Fact] public async Task Post_ValidAlerta_Returns201_WithLocation_AndRetrievable() { }
[Fact] public async Task GetById_UnknownId_Returns404_WithErrorEnvelope() { }
[Fact] public async Task Get_WithZonaFilter_ReturnsPaginatedResults() { }
```
- POST uses a Bearer token from the factory helper; assert `Location` header, then GET it.
- 404 asserts the `ErrorResponseDTO` JSON shape.

### TEST
```bash
dotnet test --filter "FullyQualifiedName~AlertaControllerTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** all three pass; the created resource is retrievable.

### COMMIT
```bash
git add tests/
git commit -m "🧪 test(api): Alerta POST/GET happy path + 404 (task N-30)"
```

---

## N-31 · confirmar + delete + auth integration tests

**Objective:** Prove the PUT confirm flow, DELETE→404 flow, and that protected routes reject missing tokens.

**GS Requirement:** Requisitos técnicos → boas práticas (auth + state change).

**Complexity:** M

**Blocks:** —
**Requires:** N-29, N-23, N-18
**Can parallelize with:** N-30, N-32

### IMPLEMENT
**Modify/Create:** `tests/.../Integration/AlertaControllerTests.cs`
```csharp
[Fact] public async Task Put_Confirmar_SetsConfirmadoTrue() { }
[Fact] public async Task Delete_Then_GetById_Returns404() { }
[Fact] public async Task Post_WithoutToken_Returns401() { }
[Fact] public async Task Get_WithoutToken_Returns200() { } // reads are public
```

### TEST
```bash
dotnet test --filter "FullyQualifiedName~AlertaControllerTests" --logger "console;verbosity=minimal"
```
**Task is complete only when:** confirm toggles the flag, delete removes the record (subsequent GET 404), and protected verbs require a token while GETs don't.

### COMMIT
```bash
git add tests/
git commit -m "🧪 test(api): confirmar/delete + JWT enforcement (task N-31)"
```

---

## N-32 · Estatísticas integration + migration smoke test

**Objective:** Prove the aggregate endpoint returns correct numbers and that the EF migration produces a valid schema (the discipline centerpiece, asserted in CI-style).

**GS Requirement:** Viabilidade e Inovação + Migration correta.

**Complexity:** M

**Blocks:** —
**Requires:** N-29, N-24
**Can parallelize with:** N-30, N-31

### IMPLEMENT
**Create:** `tests/.../Integration/EstatisticasControllerTests.cs`, `tests/.../Integration/MigrationSmokeTests.cs`
```csharp
[Fact] public async Task GetZona_Returns200_WithCorrectTotalsForSeededData() { }
[Fact] public async Task GetResumo_ZonaComMaisAlertas_IsCentro() { }      // per seeder
// Migration smoke (relational integrity over SQLite-created schema)
[Fact] public async Task Schema_Has1NRelationship_DeletingZonaWithAlertas_IsBlocked() { }
```
- The migration smoke test asserts the 1:N is enforced: inserting an alerta with a non-existent `ZonaId` fails, and deleting a zona that has alertas is rejected (Restrict).
- ASSUMPTION: full Oracle-DDL validation is a manual gate (run `dotnet ef database update` against the Oracle container in N-10); the automated smoke test runs over SQLite to keep tests hermetic. Both are required for the grade.

### TEST
```bash
dotnet test --filter "Category=Integration" --logger "console;verbosity=minimal"
```
**Task is complete only when:** statistics match the seeded fixture and the relationship constraints are enforced.

### COMMIT
```bash
git add tests/
git commit -m "🧪 test(api): statistics correctness + 1:N migration smoke (task N-32)"
```

---

## N-33 · Dockerfile (multi-stage, non-root, EXPOSE 5000)

**Objective:** Produce the exact Dockerfile Clayton drops into the compose file with zero modifications.

**GS Requirement:** Apresentação (Docker demo) + DevOps discipline penalties avoided (non-root, EXPOSE, WORKDIR).

**Complexity:** M

**Blocks:** N-34
**Requires:** all code tasks build clean
**Can parallelize with:** —

### READ BEFORE STARTING
- `CONTEXT.md` → "pulso-dotnet/Dockerfile" (the stub: alpine aspnet 8, non-root `pulso`, EXPOSE 5000, entrypoint `PulsoUrbano.Net.dll`).
- `CONTEXT.md` → docker-compose `dotnet-api` service (`ASPNETCORE_URLS=http://+:5000`, `user:"1001"`, `working_dir:/app`).

### IMPLEMENT
**Create:** `pulso-dotnet/Dockerfile` (or repo-root `Dockerfile` matching the compose `context: ./pulso-dotnet`)
**Full implementation spec — multi-stage:**
```dockerfile
# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY PulsoUrbano.Net.slnx .
COPY PulsoUrbano.Net/PulsoUrbano.Net.csproj PulsoUrbano.Net/
RUN dotnet restore PulsoUrbano.Net/PulsoUrbano.Net.csproj
COPY PulsoUrbano.Net/ PulsoUrbano.Net/
RUN dotnet publish PulsoUrbano.Net/PulsoUrbano.Net.csproj -c Release -o /app/publish --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
RUN addgroup -S pulso && adduser -S pulso -G pulso
COPY --from=build /app/publish .
USER pulso
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "PulsoUrbano.Net.dll"]
```
- ASSUMPTION: container runs the published app and applies migrations via the Development startup hook **only locally**; in cloud, migrations are applied once via `dotnet ef database update` against the Oracle service (documented in N-35 How-to). For the GS Docker demo the seeded data path is the local compose.
- Quality gates: non-root `pulso` (matches compose `user:"1001"` intent — note: alpine `adduser -S` assigns a uid; if the rubric checks `whoami` it returns `pulso`, satisfying the "not root" rule), `EXPOSE 5000`, `WORKDIR /app`.
- **What NOT to do:** no `USER root` at runtime; no secrets baked into the image (all via env).

### TEST
```bash
cd pulso-dotnet
docker build -t pulso-dotnet:gs .
docker run --rm -e DB_HOST=host.docker.internal -p 5000:5000 pulso-dotnet:gs &
sleep 8 && curl -s http://localhost:5000/api/health
docker run --rm pulso-dotnet:gs whoami   # expect: pulso  (not root)
```
**Task is complete only when:** image builds, container serves `/api/health`, and `whoami` is `pulso`.

### COMMIT
```bash
git add pulso-dotnet/Dockerfile
git commit -m "🚀 build(docker): multi-stage non-root Dockerfile, EXPOSE 5000 (task N-33)"
```

---

## N-34 · .env.example + environment variable manifest (Clayton handoff)

**Objective:** Give Clayton the exact env vars to wire the compose service — the DevOps containerization input.

**GS Requirement:** Apresentação (Docker) + DevOps requirement (env var per container).

**Complexity:** S

**Blocks:** N-35
**Requires:** N-33
**Can parallelize with:** —

### READ BEFORE STARTING
- `CONTEXT.md` → docker-compose `dotnet-api` env block.
- `Program.cs` env-var reads (N-09 connection composition, N-17 JWT).

### IMPLEMENT
**Create:** `pulso-dotnet/.env.example`
```dotenv
# Oracle connection (composed in Program.cs)
DB_USER=system
DB_PASS=oracle
DB_HOST=oracle            # service name on pulso-net in compose
DB_PORT=1521
DB_SERVICE=XEPDB1
# Shared JWT secret — MUST equal the Java API's JWT_SECRET
JWT_SECRET=pulso-secret-2026
# Kestrel
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Production
```
- See the consolidated manifest at the end of this document.
- **What NOT to do:** never commit a real `.env`; only `.env.example`. Add `.env` to `.gitignore`.

### TEST
```bash
docker run --rm --env-file pulso-dotnet/.env.example -p 5000:5000 pulso-dotnet:gs &
sleep 8 && curl -s http://localhost:5000/api/health
```
**Task is complete only when:** the container starts from `.env.example` and serves health. (Confirms no missing env var.)

### COMMIT
```bash
git add pulso-dotnet/.env.example .gitignore
git commit -m "🔧 chore(env): .env.example + env manifest for Clayton (task N-34)"
```

---

## N-35 · README.md (diagram + test examples + access instructions + pitch link) + Bosak QA template

**Objective:** Centralize everything the GS jury and Bosak need: architecture diagram, How-to from clone to running, endpoint test examples, video pitch link, and a QA issue template — the Documentação Github criterion.

**GS Requirement:** Documentação Github (10pts) + Apresentação (30pts).

**Complexity:** M

**Blocks:** — (final task)
**Requires:** N-28, N-34
**Can parallelize with:** —

### READ BEFORE STARTING
- All prior tasks (endpoints, env vars, Dockerfile, migration commands).
- GS PDF → ".NET Advanced Business Development" → README must have "diagramas, desenvolvimento e parte de testes" + "instruções para acesso e exemplos de testes".

### IMPLEMENT
**Create:** `pulso-dotnet/README.md`, `.github/ISSUE_TEMPLATE/qa_bug.md`
**README sections (in order):**
1. **Visão geral** — what the .NET API does and its boundary vs Java (one paragraph).
2. **Arquitetura** — a Mermaid (or draw.io exported PNG) diagram: client → .NET API (5000) → Oracle (1521); show ZonaReferencia 1:N AlertaHistorico.
3. **Stack** — ASP.NET Core 10, EF Core 8, Oracle, Swashbuckle, xUnit.
4. **Endpoints** — table of the 6 routes with verb, auth, sample request/response (use the contract examples).
5. **Como rodar (How-to, do clone até rodar):**
   ```bash
   git clone <repo> && cd pulso-dotnet
   cp .env.example .env            # ajuste DB_* e JWT_SECRET
   docker compose up -d            # ou: dotnet ef database update && dotnet run
   curl http://localhost:5000/api/health
   # Swagger: http://localhost:5000/swagger
   ```
6. **Migrations** — the `dotnet ef migrations add/update/list` commands + how to add a new migration.
7. **Testes** — `dotnet test` (unit) and `dotnet test --filter "Category=Integration"`, with expected output.
8. **Exemplos de teste de endpoint** — curl for POST (with Bearer), GET, PUT confirmar, DELETE, estatísticas.
9. **Vídeo pitch (3 min)** — placeholder link (Felipe records; reference here).
10. **Equipe** — RM 562999 etc.
- **Bosak QA template** (`qa_bug.md`): fields **Passos para reproduzir / Resultado esperado / Resultado atual / Endpoint + status code / Screenshot**. This is the structured handoff the team brief assigns to Bosak.
- **What NOT to do:** the architecture diagram must be draw.io/Mermaid style (DevOps wants draw.io; .NET README wants diagrams) — **never TOGAF** (TOGAF is Bosak's separate QA discipline; using it in the DevOps/README context zeroes that DevOps criterion per the GS penalties).

### TEST
```bash
# Reviewer dry-run: follow README from scratch in a clean clone
markdownlint pulso-dotnet/README.md || true   # optional
```
**Task is complete only when:** a developer with intermediate ASP.NET knowledge can go from `git clone` to a running `/swagger` using only the README, and the QA template is committed under `.github/ISSUE_TEMPLATE/`.

### COMMIT
```bash
git add pulso-dotnet/README.md .github/ISSUE_TEMPLATE/qa_bug.md
git commit -m "📝 docs(readme): full README (diagram, how-to, tests, pitch) + QA template (task N-35)"
```

---

## OPEN QUESTIONS

- **Q-01 — JWT secret/algorithm mismatch between Java and .NET.** docker-compose sets `JWT_SECRET=${JWT_SECRET:-pulso-secret-2026}` for **both** services, but `application.properties` defaults Java to `pulso-urbano-secret-key-2026-gs-fiap`. Suggested resolution: in deploy, **always** export a single explicit `JWT_SECRET` env var so both APIs use the same value; never rely on the per-app defaults. Also confirm with Felipe that JJWT signs with **HMAC-SHA256** (symmetric) — N-17 assumes HMAC. If Java uses RSA, N-17 must validate with Java's public key instead. → **Owner: Felipe (tech lead), before N-17 ships.**
- **Q-02 — Issuer/Audience validation.** Contract doesn't specify the JWT `iss`/`aud`. Suggested resolution: set `Jwt:Issuer=pulso-urbano-java`, `Jwt:Audience=pulso-urbano-clients` (from CONTEXT.md appsettings) and have Java mint tokens with those claims; otherwise set `ValidateIssuer=false`/`ValidateAudience=false` in N-17 to avoid false 401s. Default to validation off until Java confirms the claims.
- **Q-03 — Does .NET need its own copy of zona data, or read Java's `ZONA_CIDADE`?** CONTEXT.md says `ZONA_REFERENCIA_NET` is a separate reference table ("não FK cross-API"). Confirmed in scope; the seeder owns this data. No cross-API FK. (Resolved by CONTEXT.md — listed for transparency.)
- **Q-04 — Soft vs hard delete for alertas.** Entity has no `Ativo` flag and the contract returns `204`. N-20 decides **hard delete**. If the jury expects soft delete (audit trail), add an `Ativo` column + migration — a 1-task addition. → **Owner: Felipe, optional.**
- **Q-05 — Who creates `AlertaHistorico` rows in production?** The .NET API exposes `POST /api/alertas` (manual/QA/seed). In a real flow, Java's scheduler would push alerts when a score is CRITICO — but cross-API write coupling is **out of scope** for the sprint. Suggested resolution: mention in the pitch as future work; for the demo, the seeder + Swagger POST populate the history. **Do not implement Java→.NET HTTP calls.**

---

## GS REQUIREMENT COVERAGE

| Criterion              | Points | Tasks                                              | Coverage status |
|------------------------|--------|----------------------------------------------------|-----------------|
| Viabilidade e Inovação | 10     | N-11 (seed), N-15, N-21, N-24, N-32                 | ✅ Full — alert history answers "qual zona teve mais emergências; a tendência melhora?" |
| Requisitos técnicos — API REST boas práticas | (part of 50) | N-23, N-24, N-25, N-26, N-12, N-13, N-14, N-16, N-19 | ✅ Full |
| Requisitos técnicos — Persistência relacional | (part of 50) | N-05–N-11 | ✅ Full |
| Requisitos técnicos — Relacionamento 1:N | (part of 50) | N-05, N-06, N-07, N-08 (config), N-32 (proof) | ✅ Full — ZonaReferencia → AlertaHistorico (Restrict) |
| Requisitos técnicos — Migration correta | (part of 50) | N-09, N-10, N-11, N-32 | ✅ Full — InitialCreate applied to Oracle |
| Documentação Github    | 10     | N-27, N-28, N-35                                    | ✅ Full — README + Swagger |
| Apresentação           | 30     | N-27/N-28 (Swagger live), N-33 (Docker), N-35 (How-to) | ✅ Via Swagger + Docker demo |
| Segurança (JWT)        | n/a (boas práticas) | N-17, N-18, N-31 | ✅ Validation only (no generation) |

> Every task maps to ≥1 criterion. No task duplicates a Java responsibility (boundary enforced in scope notes).

---

## ENVIRONMENT VARIABLES MANIFEST

| Variable | Required | Used by | Description | Example |
|----------|----------|---------|-------------|---------|
| `DB_USER` | yes | N-09 conn string | Oracle username | `system` |
| `DB_PASS` | yes | N-09 conn string | Oracle password (never commit) | `oracle` |
| `DB_HOST` | yes | N-09 conn string | Oracle host / compose service name | `oracle` |
| `DB_PORT` | no (default 1521) | N-09 | Oracle listener port | `1521` |
| `DB_SERVICE` | no (default XEPDB1) | N-09 | Oracle service/PDB name | `XEPDB1` |
| `JWT_SECRET` | yes | N-17 | **Must equal Java's** `JWT_SECRET` (HMAC key) | `pulso-secret-2026` |
| `ASPNETCORE_URLS` | yes (Docker) | Kestrel | Bind URL inside container | `http://+:5000` |
| `ASPNETCORE_ENVIRONMENT` | yes | Program.cs | `Development` (auto-migrate+seed) / `Production` / `Testing` | `Production` |

---

*Backlog gerado para execução sequencial. Ordem em "EXECUTION ORDER". Um dev de nível intermediário em ASP.NET Core deve conseguir executar cada task sem perguntas. Resolver Q-01/Q-02 com o Felipe antes de N-17.*
