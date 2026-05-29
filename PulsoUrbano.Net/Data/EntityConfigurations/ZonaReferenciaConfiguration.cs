using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsoUrbano.Net.Models.Entities;

namespace PulsoUrbano.Net.Data.EntityConfigurations;

public class ZonaReferenciaConfiguration : IEntityTypeConfiguration<ZonaReferencia>
{
    private readonly bool _useSequences;

    public ZonaReferenciaConfiguration(bool useSequences = true) => _useSequences = useSequences;

    public void Configure(EntityTypeBuilder<ZonaReferencia> e)
    {
        e.ToTable("ZONA_REFERENCIA_NET");

        e.HasKey(z => z.Id);
        var idProp = e.Property(z => z.Id).HasColumnName("ID_ZONA");
        if (_useSequences)
            idProp.UseHiLo("SEQ_ZONA_REFERENCIA");

        e.Property(z => z.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
        e.Property(z => z.Municipio).HasColumnName("MUNICIPIO").HasMaxLength(100).IsRequired();
    }
}
