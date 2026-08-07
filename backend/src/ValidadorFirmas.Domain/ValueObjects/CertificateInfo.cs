using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>Datos del certificado del firmante y el resultado de evaluar su vigencia y cadena.</summary>
public sealed record CertificateInfo(
    string Emisor,
    string AutoridadCertificadora,
    DateTimeOffset FechaEmision,
    DateTimeOffset FechaExpiracion,
    string NumeroSerie,
    string Thumbprint,
    CertificateStatus Estado,
    ChainValidationInfo Cadena,
    RevocationInfo Revocacion);
