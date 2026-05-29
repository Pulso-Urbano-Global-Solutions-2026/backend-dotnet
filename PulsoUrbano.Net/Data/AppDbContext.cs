using Microsoft.EntityFrameworkCore;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AlertaHistorico> AlertasHistorico { get; set; }
    public DbSet<ZonaReferencia> ZonasReferencia { get; set; }
}
