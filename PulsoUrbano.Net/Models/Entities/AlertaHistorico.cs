namespace PulsoUrbano.Net.Models.Entities;

public class AlertaHistorico
{
    public int Id { get; set; }
    public int ZonaId { get; set; }
    // String (not enum) — Oracle VARCHAR2(15); allowed values validated at DTO level (N-12): ATENCAO|ALERTA|EMERGENCIA
    public string NivelAlerta { get; set; } = string.Empty;
    public double ScoreRegistrado { get; set; }
    public double No2Registrado { get; set; }
    public string TextoRecomendacao { get; set; } = string.Empty;
    public DateTime DtAlerta { get; set; } = DateTime.UtcNow;
    public bool Confirmado { get; set; } = false;
    public ZonaReferencia Zona { get; set; } = null!;
}
