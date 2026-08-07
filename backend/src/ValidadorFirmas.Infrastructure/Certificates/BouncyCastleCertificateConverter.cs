using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ValidadorFirmas.Infrastructure.Certificates;

/// <summary>Convierte certificados entre las representaciones de .NET (X509Certificate2) y BouncyCastle.</summary>
internal static class BouncyCastleCertificateConverter
{
    private static readonly X509CertificateParser Parser = new();

    public static BcX509Certificate ToBouncyCastle(this X509Certificate2 certificate) =>
        Parser.ReadCertificate(certificate.RawData);
}
