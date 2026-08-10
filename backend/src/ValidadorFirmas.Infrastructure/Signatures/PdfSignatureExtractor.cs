using System.Security.Cryptography.X509Certificates;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Signatures;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common.Models;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.ValueObjects;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Infrastructure.Signatures;

/// <summary>
/// Extrae y verifica criptográficamente las firmas de un PDF usando iText 8
/// (<see cref="SignatureUtil"/> / <see cref="PdfPKCS7"/>), incluyendo el sello de tiempo
/// RFC 3161 si está presente (iText valida su imprint internamente).
/// </summary>
public sealed class PdfSignatureExtractor : IPdfSignatureExtractor
{
    private readonly ILogger<PdfSignatureExtractor> _logger;

    public PdfSignatureExtractor(ILogger<PdfSignatureExtractor> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<RawSignatureExtraction> Extract(Stream pdfStream)
    {
        using var reader = new PdfReader(pdfStream);
        using var pdfDocument = new PdfDocument(reader);

        if (ContainsAutomaticActions(pdfDocument))
        {
            throw new DomainException(
                "El documento contiene acciones automáticas embebidas (apertura automática, JavaScript " +
                "o ejecución de programas externos) y fue rechazado por seguridad.");
        }

        var signatureUtil = new SignatureUtil(pdfDocument);
        var signatureNames = signatureUtil.GetSignatureNames();

        if (signatureNames.Count == 0)
            throw new DomainException("El documento no contiene firmas digitales.");

        var totalRevisions = signatureUtil.GetTotalRevisions();
        var extractions = new List<RawSignatureExtraction>(signatureNames.Count);

        foreach (var name in signatureNames)
        {
            var pkcs7 = signatureUtil.ReadSignatureData(name);

            var signingCertificate = X509CertificateLoader.LoadCertificate(pkcs7.GetSigningCertificate().GetEncoded());
            var chainCertificates = pkcs7.GetCertificates()
                .Select(c => X509CertificateLoader.LoadCertificate(c.GetEncoded()))
                .ToList();

            extractions.Add(new RawSignatureExtraction(
                NombreCampoFirma: name,
                CertificadoFirmante: signingCertificate,
                CadenaCertificadosCms: chainCertificates,
                FechaFirmaReclamada: ToDateTimeOffset(pkcs7.GetSignDate()),
                AlgoritmoResumen: pkcs7.GetDigestAlgorithmName(),
                AlgoritmoFirma: pkcs7.GetSignatureAlgorithmName(),
                IntegridadCriptograficaValida: VerifyIntegrity(pkcs7, name),
                CubreDocumentoCompleto: signatureUtil.SignatureCoversWholeDocument(name),
                NumeroRevision: signatureUtil.GetRevision(name),
                TotalRevisiones: totalRevisions,
                Timestamp: ExtractTimestamp(pkcs7, name)));
        }

        return extractions;
    }

    private bool VerifyIntegrity(PdfPKCS7 pkcs7, string signatureName)
    {
        try
        {
            return pkcs7.VerifySignatureIntegrityAndAuthenticity();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo verificar la integridad de la firma {SignatureName}", signatureName);
            return false;
        }
    }

    private TimestampInfo ExtractTimestamp(PdfPKCS7 pkcs7, string signatureName)
    {
        var timestampDate = pkcs7.GetTimeStampDate();
        if (timestampDate == TimestampConstants.UNDEFINED_TIMESTAMP_DATE)
            return new TimestampInfo(Presente: false, FechaHora: null, AutoridadSellado: null, Valido: null);

        bool? valido;
        try
        {
            valido = pkcs7.VerifyTimestampImprint();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo verificar el sello de tiempo de la firma {SignatureName}", signatureName);
            valido = null;
        }

        string? autoridadSellado = null;
        try
        {
            var timestampCertificates = pkcs7.GetTimestampCertificates();
            if (timestampCertificates is { Length: > 0 })
            {
                var tsaCertificate = X509CertificateLoader.LoadCertificate(timestampCertificates[0].GetEncoded());
                autoridadSellado = tsaCertificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el certificado de la autoridad de sellado de tiempo de {SignatureName}", signatureName);
        }

        return new TimestampInfo(
            Presente: true,
            FechaHora: ToDateTimeOffset(timestampDate),
            AutoridadSellado: autoridadSellado,
            Valido: valido);
    }

    /// <summary>
    /// Chequeo estructural best-effort contra vectores clásicos de PDF malicioso: apertura
    /// automática (/OpenAction), acciones automáticas de documento/página (/AA) o ejecución de
    /// programas externos (acciones /Launch en anotaciones). Es un complemento, no un reemplazo
    /// de un antivirus real — un documento sin estas construcciones no está garantizado inocuo.
    /// </summary>
    private static bool ContainsAutomaticActions(PdfDocument pdfDocument)
    {
        var catalogDict = pdfDocument.GetCatalog().GetPdfObject();
        if (catalogDict.ContainsKey(PdfName.OpenAction) || catalogDict.ContainsKey(PdfName.AA))
            return true;

        for (var i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            if (page.GetPdfObject().ContainsKey(PdfName.AA))
                return true;

            foreach (PdfAnnotation annotation in page.GetAnnotations())
            {
                var action = annotation.GetPdfObject().GetAsDictionary(PdfName.A);
                if (action is not null && PdfName.Launch.Equals(action.GetAsName(PdfName.S)))
                    return true;
            }
        }

        return false;
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime dateTime) => dateTime.Kind switch
    {
        DateTimeKind.Utc => new DateTimeOffset(dateTime),
        DateTimeKind.Local => new DateTimeOffset(dateTime),
        _ => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local))
    };
}
