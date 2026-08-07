using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Domain.Entities;

/// <summary>
/// Resultado completo de validar una firma digital individual dentro de un PDF.
/// Un documento puede contener varias (ver <see cref="Domain.DocumentValidationResult"/>).
/// </summary>
public sealed class SignatureValidation
{
    public Guid Id { get; }
    public string NombreCampoFirma { get; }
    public SignerInfo Firmante { get; }
    public CertificateInfo Certificado { get; }
    public DateTimeOffset? FechaFirma { get; }
    public string AlgoritmoResumen { get; }
    public string AlgoritmoFirma { get; }
    public bool IntegridadCriptograficaValida { get; }
    public bool CubreDocumentoCompleto { get; }

    /// <summary>
    /// Indica si esta es la última firma aplicada al documento (última revisión incremental).
    /// Solo para la última firma es una anomalía que no cubra el documento completo: en firmas
    /// múltiples es normal que las firmas anteriores no cubran las revisiones agregadas después.
    /// </summary>
    public bool EsUltimaRevision { get; }
    public TimestampInfo Timestamp { get; }
    public OverallStatus Estado { get; }
    public string? Motivo { get; }

    public SignatureValidation(
        string nombreCampoFirma,
        SignerInfo firmante,
        CertificateInfo certificado,
        DateTimeOffset? fechaFirma,
        string algoritmoResumen,
        string algoritmoFirma,
        bool integridadCriptograficaValida,
        bool cubreDocumentoCompleto,
        bool esUltimaRevision,
        TimestampInfo timestamp)
    {
        Id = Guid.NewGuid();
        NombreCampoFirma = nombreCampoFirma;
        Firmante = firmante;
        Certificado = certificado;
        FechaFirma = fechaFirma;
        AlgoritmoResumen = algoritmoResumen;
        AlgoritmoFirma = algoritmoFirma;
        IntegridadCriptograficaValida = integridadCriptograficaValida;
        CubreDocumentoCompleto = cubreDocumentoCompleto;
        EsUltimaRevision = esUltimaRevision;
        Timestamp = timestamp;

        (Estado, Motivo) = Evaluar();
    }

    private (OverallStatus, string?) Evaluar()
    {
        if (!IntegridadCriptograficaValida)
            return (OverallStatus.Invalido, "La firma es inválida.");

        if (EsUltimaRevision && !CubreDocumentoCompleto)
            return (OverallStatus.Invalido, "El documento fue modificado después de firmarse.");

        if (Certificado.Estado == CertificateStatus.Revocado)
            return (OverallStatus.Invalido, "El certificado fue revocado.");

        if (Certificado.Estado == CertificateStatus.Expirado)
            return (OverallStatus.Invalido, "El certificado expiró.");

        if (Certificado.Cadena.Estado == ChainStatus.Incorrecta)
            return (OverallStatus.Invalido, "No existe confianza en la cadena de certificación.");

        if (Certificado.Cadena.Estado == ChainStatus.NoVerificable ||
            Certificado.Revocacion.Estado == RevocationStatus.NoVerificable)
            return (OverallStatus.Advertencia, "No se pudo verificar completamente la cadena de confianza o el estado de revocación.");

        return (OverallStatus.Valido, null);
    }
}
