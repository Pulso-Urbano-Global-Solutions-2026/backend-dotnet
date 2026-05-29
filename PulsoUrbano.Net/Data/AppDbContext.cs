using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Data.EntityConfigurations;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AlertaHistorico> AlertasHistorico => Set<AlertaHistorico>();
    public DbSet<ZonaReferencia> ZonasReferencia => Set<ZonaReferencia>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // SQLite (tests) does not support sequences; Oracle uses HiLo
        bool useSequences = Database.ProviderName == "Oracle.EntityFrameworkCore";
        mb.ApplyConfiguration(new ZonaReferenciaConfiguration(useSequences));
        mb.ApplyConfiguration(new AlertaHistoricoConfiguration(useSequences));
    }
}
