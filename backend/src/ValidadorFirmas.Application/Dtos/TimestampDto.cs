namespace ValidadorFirmas.Application.Dtos;

public sealed record TimestampDto(
    bool Presente,
    DateTimeOffset? FechaHora,
    string? AutoridadSellado,
    bool? Valido);
