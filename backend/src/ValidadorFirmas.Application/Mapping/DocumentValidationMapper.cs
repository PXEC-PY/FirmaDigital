using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Application.Mapping;

/// <summary>Traduce las entidades del dominio de validación a los DTOs expuestos por la API.</summary>
public static class DocumentValidationMapper
{
    public static DocumentValidationResponseDto ToDto(this DocumentValidationResult result) => new(
        DocumentoId: result.Id,
        NombreArchivo: result.NombreArchivo,
        HashSha256: result.HashSha256,
        FechaValidacion: result.FechaValidacionUtc,
        EstadoGeneral: result.EstadoGeneral.ToString(),
        Motivo: result.Motivo,
        Documento: result.Integridad.ToDto(),
        Firmas: result.Firmas.Select(f => f.ToDto()).ToList());

    private static DocumentIntegrityDto ToDto(this DocumentIntegrity integrity) => new(
        integrity.EsIntegro,
        integrity.CantidadFirmas,
        integrity.Motivo);

    private static SignatureDto ToDto(this SignatureValidation signature) => new(
        NombreCampo: signature.NombreCampoFirma,
        Firmante: signature.Firmante.ToDto(),
        FechaFirma: signature.FechaFirma,
        AlgoritmoResumen: signature.AlgoritmoResumen,
        AlgoritmoFirma: signature.AlgoritmoFirma,
        NumeroSerie: signature.Certificado.NumeroSerie,
        Thumbprint: signature.Certificado.Thumbprint,
        Certificado: signature.Certificado.ToDto(),
        Timestamp: signature.Timestamp.ToDto(),
        IntegridadCriptograficaValida: signature.IntegridadCriptograficaValida,
        CubreDocumentoCompleto: signature.CubreDocumentoCompleto,
        Estado: signature.Estado.ToString(),
        Motivo: signature.Motivo);

    private static SignerDto ToDto(this SignerInfo signer) => new(
        signer.NombreCompleto,
        signer.NumeroDocumento,
        signer.Correo,
        signer.Empresa,
        signer.Cargo);

    private static CertificateDto ToDto(this CertificateInfo certificate) => new(
        Emisor: certificate.Emisor,
        AutoridadCertificadora: certificate.AutoridadCertificadora,
        FechaEmision: certificate.FechaEmision,
        FechaExpiracion: certificate.FechaExpiracion,
        NumeroSerie: certificate.NumeroSerie,
        Thumbprint: certificate.Thumbprint,
        Estado: certificate.Estado.ToString(),
        Cadena: certificate.Cadena.ToDto(),
        Revocacion: certificate.Revocacion.ToDto());

    private static ChainDto ToDto(this ChainValidationInfo chain) => new(
        chain.Estado.ToString(),
        chain.Motivo,
        chain.CadenaEmisores);

    private static RevocationDto ToDto(this RevocationInfo revocation) => new(
        revocation.Estado.ToString(),
        revocation.Fuente.ToString(),
        revocation.FechaConsulta,
        revocation.Motivo);

    private static TimestampDto ToDto(this TimestampInfo timestamp) => new(
        timestamp.Presente,
        timestamp.FechaHora,
        timestamp.AutoridadSellado,
        timestamp.Valido);
}
