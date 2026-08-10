using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;
using ValidadorFirmas.Infrastructure.Certificates;
using ValidadorFirmas.Infrastructure.Options;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ValidadorFirmas.Infrastructure.Revocation;

/// <summary>
/// Consulta el estado de revocación de un certificado: primero OCSP (si el certificado publica
/// un responder en su extensión Authority Information Access), y si no está disponible o no se
/// puede verificar su firma, recurre a CRL —local primero, luego el CRL Distribution Point del
/// certificado—. Nunca confía en una respuesta cuya firma no pudo verificarse contra el emisor.
/// </summary>
public sealed class RevocationChecker : IRevocationChecker
{
    private readonly LocalCrlStore _localCrlStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TrustStoreOptions _options;
    private readonly ILogger<RevocationChecker> _logger;

    public RevocationChecker(
        LocalCrlStore localCrlStore,
        IHttpClientFactory httpClientFactory,
        IOptions<TrustStoreOptions> options,
        ILogger<RevocationChecker> logger)
    {
        _localCrlStore = localCrlStore;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RevocationInfo> CheckRevocationAsync(
        X509Certificate2 certificate, X509Certificate2? issuer, DateTimeOffset referenceTime, CancellationToken cancellationToken)
    {
        if (issuer is null)
        {
            return new RevocationInfo(
                RevocationStatus.NoVerificable,
                RevocationSource.Ninguna,
                DateTimeOffset.UtcNow,
                "No se pudo determinar el certificado emisor para consultar la revocación.");
        }

        var bcCertificate = certificate.ToBouncyCastle();
        var bcIssuer = issuer.ToBouncyCastle();

        var ocspResult = await TryOcspAsync(bcCertificate, bcIssuer, certificate, cancellationToken);
        if (ocspResult is not null)
            return ocspResult;

        var crlResult = await TryCrlAsync(bcCertificate, bcIssuer, certificate, referenceTime, cancellationToken);
        if (crlResult is not null)
            return crlResult;

        return new RevocationInfo(
            RevocationStatus.NoVerificable,
            RevocationSource.Ninguna,
            DateTimeOffset.UtcNow,
            "No hay OCSP ni CRL disponibles para verificar la revocación de este certificado.");
    }

    private async Task<RevocationInfo?> TryOcspAsync(
        BcX509Certificate bcCertificate, BcX509Certificate bcIssuer, X509Certificate2 certificate, CancellationToken cancellationToken)
    {
        var ocspUrl = certificate.Extensions
            .OfType<X509AuthorityInformationAccessExtension>()
            .FirstOrDefault()?.EnumerateOcspUris().FirstOrDefault();

        if (string.IsNullOrEmpty(ocspUrl))
            return null;

        if (!await SsrfGuard.IsUrlSafeAsync(ocspUrl, _logger, cancellationToken))
            return null;

        try
        {
            var certificateId = new CertificateID(Asn1DigestFactory.Get("SHA1"), bcIssuer, bcCertificate.SerialNumber);
            var requestGenerator = new OcspReqGenerator();
            requestGenerator.AddRequest(certificateId);
            var ocspRequest = requestGenerator.Generate();

            var client = _httpClientFactory.CreateClient(nameof(RevocationChecker));
            client.Timeout = TimeSpan.FromSeconds(_options.RemoteTimeoutSeconds);
            using var content = new ByteArrayContent(ocspRequest.GetEncoded());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/ocsp-request");

            using var response = await client.PostAsync(ocspUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var ocspResponse = new OcspResp(responseBytes);
            if (ocspResponse.Status != 0 || ocspResponse.GetResponseObject() is not BasicOcspResp basicResponse)
                return null;

            if (!VerifyOcspSignature(basicResponse, bcIssuer))
            {
                _logger.LogWarning("La respuesta OCSP de {Url} no pudo verificarse contra el certificado emisor; se ignora.", ocspUrl);
                return null;
            }

            var singleResponse = basicResponse.Responses
                .FirstOrDefault(r => r.GetCertID().SerialNumber.Equals(bcCertificate.SerialNumber));
            if (singleResponse is null)
                return null;

            var consultedAt = DateTimeOffset.UtcNow;
            return singleResponse.GetCertStatus() switch
            {
                null => new RevocationInfo(RevocationStatus.NoRevocado, RevocationSource.Ocsp, consultedAt, null),
                RevokedStatus revoked => new RevocationInfo(
                    RevocationStatus.Revocado, RevocationSource.Ocsp, consultedAt,
                    $"El certificado fue revocado el {revoked.RevocationTime:dd/MM/yyyy}."),
                _ => null // estado desconocido: se intenta con CRL
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al consultar el responder OCSP {Url}", ocspUrl);
            return null;
        }
    }

    private static bool VerifyOcspSignature(BasicOcspResp basicResponse, BcX509Certificate issuer)
    {
        try
        {
            return basicResponse.Verify(issuer.GetPublicKey());
        }
        catch
        {
            return false;
        }
    }

    private async Task<RevocationInfo?> TryCrlAsync(
        BcX509Certificate bcCertificate, BcX509Certificate bcIssuer, X509Certificate2 certificate,
        DateTimeOffset referenceTime, CancellationToken cancellationToken)
    {
        var crl = _localCrlStore.FindByIssuer(bcIssuer.SubjectDN);

        if (crl is null)
        {
            var crlUrl = ExtractCrlDistributionPointUrl(certificate);
            if (crlUrl is not null)
                crl = await TryDownloadCrlAsync(crlUrl, cancellationToken);
        }

        if (crl is null)
            return null;

        if (!VerifyCrlSignature(crl, bcIssuer))
        {
            _logger.LogWarning("La firma de la CRL del emisor {Issuer} no pudo verificarse; se ignora.", bcIssuer.SubjectDN);
            return null;
        }

        var entry = crl.GetRevokedCertificate(bcCertificate.SerialNumber);
        var consultedAt = DateTimeOffset.UtcNow;

        if (entry is null)
            return new RevocationInfo(RevocationStatus.NoRevocado, RevocationSource.Crl, consultedAt, null);

        var revocationDate = new DateTimeOffset(DateTime.SpecifyKind(entry.RevocationDate, DateTimeKind.Utc));

        if (revocationDate <= referenceTime)
        {
            return new RevocationInfo(
                RevocationStatus.Revocado, RevocationSource.Crl, consultedAt,
                $"El certificado fue revocado el {revocationDate:dd/MM/yyyy}.");
        }

        return new RevocationInfo(
            RevocationStatus.NoRevocado, RevocationSource.Crl, consultedAt,
            $"El certificado fue revocado el {revocationDate:dd/MM/yyyy}, posterior a la fecha de firma; no afecta la validez de esta firma.");
    }

    private static bool VerifyCrlSignature(X509Crl crl, BcX509Certificate issuer)
    {
        try
        {
            crl.Verify(issuer.GetPublicKey());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<X509Crl?> TryDownloadCrlAsync(string url, CancellationToken cancellationToken)
    {
        if (!await SsrfGuard.IsUrlSafeAsync(url, _logger, cancellationToken))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(RevocationChecker));
            client.Timeout = TimeSpan.FromSeconds(_options.RemoteTimeoutSeconds);
            var bytes = await client.GetByteArrayAsync(url, cancellationToken);
            return new X509CrlParser().ReadCrl(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo descargar la CRL remota {Url}", url);
            return null;
        }
    }

    private static string? ExtractCrlDistributionPointUrl(X509Certificate2 certificate)
    {
        var extension = certificate.Extensions["2.5.29.31"];
        if (extension is null)
            return null;

        try
        {
            var crlDistPoint = CrlDistPoint.GetInstance(Asn1Object.FromByteArray(extension.RawData));
            foreach (var distributionPoint in crlDistPoint.GetDistributionPoints())
            {
                if (distributionPoint.DistributionPointName?.Type != DistributionPointName.FullName)
                    continue;

                var names = GeneralNames.GetInstance(distributionPoint.DistributionPointName.Name);
                foreach (var name in names.GetNames())
                {
                    if (name.TagNo != GeneralName.UniformResourceIdentifier)
                        continue;

                    var uri = DerIA5String.GetInstance(name.Name).GetString();
                    if (uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        return uri;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
