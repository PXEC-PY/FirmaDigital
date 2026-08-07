using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Infrastructure.Certificates;

/// <summary>
/// Construye la cadena de confianza de un certificado usando exclusivamente las raíces
/// cargadas en <see cref="ITrustedCertificateStore"/> (no la tienda de confianza del sistema
/// operativo), tal como corresponde para una PKI nacional específica.
/// </summary>
public sealed class X509ChainValidator : ICertificateChainValidator
{
    private readonly ITrustedCertificateStore _trustedCertificateStore;
    private readonly ILogger<X509ChainValidator> _logger;

    public X509ChainValidator(ITrustedCertificateStore trustedCertificateStore, ILogger<X509ChainValidator> logger)
    {
        _trustedCertificateStore = trustedCertificateStore;
        _logger = logger;
    }

    public ChainValidationInfo ValidateChain(X509Certificate2 leaf, IReadOnlyList<X509Certificate2> intermediatesFromDocument)
    {
        var roots = _trustedCertificateStore.GetTrustedRoots();
        if (roots.Count == 0)
        {
            return new ChainValidationInfo(
                ChainStatus.NoVerificable,
                "No hay certificados raíz cargados en el almacén de confianza (TrustedCertificates/).",
                []);
        }

        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Clear();
        foreach (var root in roots)
            chain.ChainPolicy.CustomTrustStore.Add(root);

        foreach (var intermediate in intermediatesFromDocument)
            chain.ChainPolicy.ExtraStore.Add(intermediate);
        foreach (var intermediate in _trustedCertificateStore.GetIntermediateCertificates())
            chain.ChainPolicy.ExtraStore.Add(intermediate);

        // La revocación se evalúa por separado (CRL local + OCSP), combinando ambas fuentes.
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

        var built = chain.Build(leaf);
        var cadenaEmisores = chain.ChainElements
            .Cast<X509ChainElement>()
            .Skip(1)
            .Select(e => e.Certificate.Subject)
            .ToList();

        if (built)
            return new ChainValidationInfo(ChainStatus.Correcta, null, cadenaEmisores);

        var motivos = chain.ChainStatus
            .Where(s => s.Status != X509ChainStatusFlags.NoError)
            .Select(s => DescribeStatus(s.Status))
            .Distinct()
            .ToList();

        var motivo = motivos.Count > 0
            ? string.Join(" ", motivos)
            : "No se pudo construir la cadena de confianza hasta una raíz confiable.";

        _logger.LogInformation("Cadena de confianza incorrecta para {Subject}: {Motivo}", leaf.Subject, motivo);
        return new ChainValidationInfo(ChainStatus.Incorrecta, motivo, cadenaEmisores);
    }

    public X509Certificate2? FindIssuer(X509Certificate2 certificate, IReadOnlyList<X509Certificate2> knownCertificates)
    {
        var authorityKeyId = certificate.Extensions
            .OfType<X509AuthorityKeyIdentifierExtension>()
            .FirstOrDefault()?.KeyIdentifier;

        if (authorityKeyId is { } aki)
        {
            var byKeyId = knownCertificates.FirstOrDefault(candidate =>
            {
                var ski = candidate.Extensions.OfType<X509SubjectKeyIdentifierExtension>().FirstOrDefault();
                return ski is not null && ski.SubjectKeyIdentifierBytes.Span.SequenceEqual(aki.Span);
            });
            if (byKeyId is not null)
                return byKeyId;
        }

        return knownCertificates.FirstOrDefault(candidate => candidate.Subject == certificate.Issuer);
    }

    private static string DescribeStatus(X509ChainStatusFlags status) => status switch
    {
        X509ChainStatusFlags.NotTimeValid => "Un certificado de la cadena está fuera de su período de vigencia.",
        X509ChainStatusFlags.Revoked => "Un certificado de la cadena está revocado.",
        X509ChainStatusFlags.NotSignatureValid => "Un certificado de la cadena tiene una firma inválida.",
        X509ChainStatusFlags.UntrustedRoot => "La cadena no llega a una raíz confiable.",
        X509ChainStatusFlags.PartialChain => "No se pudo completar la cadena: falta un certificado intermedio.",
        X509ChainStatusFlags.NotValidForUsage => "Un certificado de la cadena no está habilitado para este uso.",
        _ => status.ToString()
    };
}
