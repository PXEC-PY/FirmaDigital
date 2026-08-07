using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>Resultado de consultar la revocación de un certificado (CRL y/o OCSP).</summary>
public sealed record RevocationInfo(
    RevocationStatus Estado,
    RevocationSource Fuente,
    DateTimeOffset? FechaConsulta,
    string? Motivo);
