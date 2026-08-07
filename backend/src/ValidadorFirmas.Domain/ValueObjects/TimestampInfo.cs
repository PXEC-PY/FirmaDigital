namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>Información del sello de tiempo (RFC 3161) asociado a una firma, si existe.</summary>
public sealed record TimestampInfo(
    bool Presente,
    DateTimeOffset? FechaHora,
    string? AutoridadSellado,
    bool? Valido);
