using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>Resultado de construir la cadena de confianza de un certificado hasta una raíz.</summary>
public sealed record ChainValidationInfo(
    ChainStatus Estado,
    string? Motivo,
    IReadOnlyList<string> CadenaEmisores);
