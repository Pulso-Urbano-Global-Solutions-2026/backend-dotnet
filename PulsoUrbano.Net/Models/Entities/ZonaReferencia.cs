namespace PulsoUrbano.Net.Models.Entities;

public class ZonaReferencia
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Municipio { get; set; } = "São Paulo";

    public ICollection<AlertaHistorico> Alertas { get; set; } = new List<AlertaHistorico>();
}
