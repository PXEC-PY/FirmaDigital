namespace ValidadorFirmas.Application.Dtos;

public sealed record SignatureDto(
    string NombreCampo,
    SignerDto Firmante,
    DateTimeOffset? FechaFirma,
    string AlgoritmoResumen,
    string AlgoritmoFirma,
    string NumeroSerie,
    string Thumbprint,
    CertificateDto Certificado,
    TimestampDto Timestamp,
    bool IntegridadCriptograficaValida,
    bool CubreDocumentoCompleto,
    string Estado,
    string? Motivo);
