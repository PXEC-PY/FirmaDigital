namespace ValidadorFirmas.Application.Dtos;

public sealed record RevocationDto(
    string Estado,
    string Fuente,
    DateTimeOffset? FechaConsulta,
    string? Motivo);
