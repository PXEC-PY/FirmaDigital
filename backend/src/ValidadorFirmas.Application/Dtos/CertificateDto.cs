namespace ValidadorFirmas.Application.Dtos;

public sealed record CertificateDto(
    string Emisor,
    string AutoridadCertificadora,
    DateTimeOffset FechaEmision,
    DateTimeOffset FechaExpiracion,
    string NumeroSerie,
    string Thumbprint,
    string Estado,
    ChainDto Cadena,
    RevocationDto Revocacion);
