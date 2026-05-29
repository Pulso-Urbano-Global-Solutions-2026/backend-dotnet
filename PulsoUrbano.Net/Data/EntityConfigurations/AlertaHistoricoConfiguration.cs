using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data.EntityConfigurations;

public class AlertaHistoricoConfiguration : IEntityTypeConfiguration<AlertaHistorico>
{
    public void Configure(EntityTypeBuilder<AlertaHistorico> e)
    {
        e.HasKey(a => a.Id);
        e.HasOne(a => a.Zona)
         .WithMany(z => z.Alertas)
         .HasForeignKey(a => a.ZonaId)
         .OnDelete(DeleteBehavior.Restrict); // deleting a Zona with alerts → blocked (N-19 maps to 409)
        // Oracle column/sequence mappings added in N-08 (same file)
    }
}
