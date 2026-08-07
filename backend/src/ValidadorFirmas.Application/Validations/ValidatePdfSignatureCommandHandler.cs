using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MediatR;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common;
using ValidadorFirmas.Application.Common.Models;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Application.Mapping;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Application.Validations;

public sealed class ValidatePdfSignatureCommandHandler
    : IRequestHandler<ValidatePdfSignatureCommand, DocumentValidationResponseDto>
{
    private readonly IPdfSignatureExtractor _extractor;
    private readonly ICertificateChainValidator _chainValidator;
    private readonly IRevocationChecker _revocationChecker;
    private readonly ITrustedCertificateStore _trustedCertificateStore;
    private readonly ILogger<ValidatePdfSignatureCommandHandler> _logger;

    public ValidatePdfSignatureCommandHandler(
        IPdfSignatureExtractor extractor,
        ICertificateChainValidator chainValidator,
        IRevocationChecker revocationChecker,
        ITrustedCertificateStore trustedCertificateStore,
        ILogger<ValidatePdfSignatureCommandHandler> logger)
    {
        _extractor = extractor;
        _chainValidator = chainValidator;
        _revocationChecker = revocationChecker;
        _trustedCertificateStore = trustedCertificateStore;
        _logger = logger;
    }

    public async Task<DocumentValidationResponseDto> Handle(
        ValidatePdfSignatureCommand request, CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(request.FileBytes));

        IReadOnlyList<RawSignatureExtraction> rawSignatures;
        try
        {
            using var stream = new MemoryStream(request.FileBytes, writable: false);
            rawSignatures = _extractor.Extract(stream);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el documento PDF {FileName}", request.FileName);
            throw new DomainException("No se pudo leer el documento. Verifique que sea un PDF válido y no esté corrupto.");
        }

        var signatures = new List<SignatureValidation>(rawSignatures.Count);
        foreach (var raw in rawSignatures)
            signatures.Add(await BuildSignatureValidationAsync(raw, cancellationToken));

        var integrity = EvaluateDocumentIntegrity(signatures);
        var result = new DocumentValidationResult(request.FileName, hash, signatures, integrity);
        return result.ToDto();
    }

    private async Task<SignatureValidation> BuildSignatureValidationAsync(
        RawSignatureExtraction raw, CancellationToken cancellationToken)
    {
        var signer = SignerInfoExtractor.Extract(raw.CertificadoFirmante);

        var intermediatesFromCms = raw.CadenaCertificadosCms
            .Where(c => !c.Equals(raw.CertificadoFirmante))
            .ToList();

        var chainInfo = _chainValidator.ValidateChain(raw.CertificadoFirmante, intermediatesFromCms);

        var knownCertificates = intermediatesFromCms
            .Concat(_trustedCertificateStore.GetIntermediateCertificates())
            .Concat(_trustedCertificateStore.GetTrustedRoots())
            .ToList();
        var issuer = _chainValidator.FindIssuer(raw.CertificadoFirmante, knownCertificates);

        var timestamp = raw.Timestamp;

        var referenceTime = timestamp is { Presente: true, Valido: true, FechaHora: not null }
            ? timestamp.FechaHora!.Value
            : raw.FechaFirmaReclamada ?? DateTimeOffset.UtcNow;

        var revocation = await _revocationChecker.CheckRevocationAsync(
            raw.CertificadoFirmante, issuer, referenceTime, cancellationToken);

        var certificateStatus = DetermineCertificateStatus(raw.CertificadoFirmante, revocation, referenceTime);
        var thumbprint = Convert.ToHexString(raw.CertificadoFirmante.GetCertHash(HashAlgorithmName.SHA256));

        var certificateInfo = new CertificateInfo(
            Emisor: raw.CertificadoFirmante.Issuer,
            AutoridadCertificadora: issuer?.GetNameInfo(X509NameType.SimpleName, forIssuer: false)
                ?? raw.CertificadoFirmante.GetNameInfo(X509NameType.SimpleName, forIssuer: true),
            FechaEmision: raw.CertificadoFirmante.NotBefore,
            FechaExpiracion: raw.CertificadoFirmante.NotAfter,
            NumeroSerie: raw.CertificadoFirmante.SerialNumber ?? string.Empty,
            Thumbprint: thumbprint,
            Estado: certificateStatus,
            Cadena: chainInfo,
            Revocacion: revocation);

        return new SignatureValidation(
            nombreCampoFirma: raw.NombreCampoFirma,
            firmante: signer,
            certificado: certificateInfo,
            fechaFirma: raw.FechaFirmaReclamada,
            algoritmoResumen: raw.AlgoritmoResumen,
            algoritmoFirma: raw.AlgoritmoFirma,
            integridadCriptograficaValida: raw.IntegridadCriptograficaValida,
            cubreDocumentoCompleto: raw.CubreDocumentoCompleto,
            esUltimaRevision: raw.NumeroRevision == raw.TotalRevisiones,
            timestamp: timestamp);
    }

    private static CertificateStatus DetermineCertificateStatus(
        X509Certificate2 certificate, RevocationInfo revocation, DateTimeOffset referenceTime)
    {
        if (revocation.Estado == RevocationStatus.Revocado)
            return CertificateStatus.Revocado;

        if (referenceTime < new DateTimeOffset(certificate.NotBefore) ||
            referenceTime > new DateTimeOffset(certificate.NotAfter))
            return CertificateStatus.Expirado;

        return CertificateStatus.Vigente;
    }

    private static DocumentIntegrity EvaluateDocumentIntegrity(IReadOnlyList<SignatureValidation> signatures)
    {
        var ultimaFirma = signatures.FirstOrDefault(s => s.EsUltimaRevision);
        var esIntegro = ultimaFirma is null || ultimaFirma.CubreDocumentoCompleto;
        var motivo = esIntegro ? null : "El documento fue modificado después de la última firma.";
        return new DocumentIntegrity(esIntegro, signatures.Count, motivo);
    }
}
