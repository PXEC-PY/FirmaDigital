namespace ValidadorFirmas.Infrastructure.Options;

/// <summary>
/// Ubicación de los certificados raíz de confianza y las listas de revocación locales.
/// Agregar una nueva Autoridad Certificadora (de Paraguay o de otro país) es copiar sus
/// archivos a estas carpetas: no requiere cambios de código.
/// </summary>
public sealed class TrustStoreOptions
{
    public const string SectionName = "TrustStore";

    /// <summary>Carpeta con certificados raíz e intermedios (.cer/.crt/.pem), relativa al content root o absoluta.</summary>
    public string RootCertificatesPath { get; set; } = "../../../TrustedCertificates";

    /// <summary>Carpeta con listas de revocación locales (.crl), relativa al content root o absoluta.</summary>
    public string CrlPath { get; set; } = "../../../CRL";

    /// <summary>Tiempo máximo de espera para consultas OCSP/CRL remotas.</summary>
    public int RemoteTimeoutSeconds { get; set; } = 8;
}
