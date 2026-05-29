using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data.EntityConfigurations;

public class AlertaHistoricoConfiguration : IEntityTypeConfiguration<AlertaHistorico>
{
    private readonly bool _useSequences;

    public AlertaHistoricoConfiguration(bool useSequences = true) => _useSequences = useSequences;

    public void Configure(EntityTypeBuilder<AlertaHistorico> e)
    {
        e.ToTable("ALERTA_HISTORICO");

        e.HasKey(a => a.Id);
        var idProp = e.Property(a => a.Id).HasColumnName("ID_ALERTA");
        if (_useSequences)
            idProp.UseHiLo("SEQ_ALERTA_HISTORICO");

        e.Property(a => a.ZonaId).HasColumnName("ID_ZONA").IsRequired();
        e.Property(a => a.NivelAlerta).HasColumnName("NIVEL_ALERTA").HasMaxLength(15).IsRequired();
        e.Property(a => a.ScoreRegistrado).HasColumnName("SCORE_REGISTRADO").HasColumnType("NUMBER(5,2)");
        e.Property(a => a.No2Registrado).HasColumnName("NO2_REGISTRADO").HasColumnType("NUMBER(8,4)");
        e.Property(a => a.TextoRecomendacao).HasColumnName("TEXTO_RECOMENDACAO").HasMaxLength(1000);
        e.Property(a => a.DtAlerta).HasColumnName("DT_ALERTA").HasColumnType("DATE");
        // bool → NUMBER(1): Oracle EF provider handles the mapping; explicit type makes DDL unambiguous
        e.Property(a => a.Confirmado).HasColumnName("CONFIRMADO").HasColumnType("NUMBER(1)");

        e.HasOne(a => a.Zona)
         .WithMany(z => z.Alertas)
         .HasForeignKey(a => a.ZonaId)
         .OnDelete(DeleteBehavior.Restrict); // deleting a Zona with alerts → blocked (N-19 maps to 409)

        e.HasIndex(a => new { a.ZonaId, a.DtAlerta }).HasDatabaseName("IX_ALERTA_ZONA_DT");
    }
}
