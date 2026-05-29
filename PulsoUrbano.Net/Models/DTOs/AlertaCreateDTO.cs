namespace PulsoUrbano.Net.Models.DTOs;

public record AlertaCreateDTO(
    int ZonaId,
    string NivelAlerta,
    double ScoreRegistrado,
    double No2Registrado,
    string TextoRecomendacao);
