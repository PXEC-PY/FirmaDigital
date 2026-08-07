using System.Security.Cryptography.X509Certificates;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Application.Common.Models;

/// <summary>
/// Datos crudos extraídos de una firma PDF (CMS/PKCS#7), antes de resolver cadena de
/// confianza y revocación. Producido por <see cref="Ports.IPdfSignatureExtractor"/>.
/// El sello de tiempo (si existe) ya viene validado: iText verifica su imprint internamente.
/// </summary>
public sealed record RawSignatureExtraction(
    string NombreCampoFirma,
    X509Certificate2 CertificadoFirmante,
    IReadOnlyList<X509Certificate2> CadenaCertificadosCms,
    DateTimeOffset? FechaFirmaReclamada,
    string AlgoritmoResumen,
    string AlgoritmoFirma,
    bool IntegridadCriptograficaValida,
    bool CubreDocumentoCompleto,
    int NumeroRevision,
    int TotalRevisiones,
    TimestampInfo Timestamp);
