using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using ValidadorFirmas.Infrastructure.Options;

namespace ValidadorFirmas.Infrastructure.Revocation;

/// <summary>
/// Carga y cachea en memoria las listas de revocación locales (carpeta CRL/). Se usan como
/// primera fuente de revocación antes de intentar descargar la CRL desde el CRL Distribution
/// Point del certificado.
/// </summary>
public sealed class LocalCrlStore
{
    private readonly Lazy<IReadOnlyList<X509Crl>> _crls;

    public LocalCrlStore(
        IOptions<TrustStoreOptions> options,
        IHostEnvironment environment,
        ILogger<LocalCrlStore> logger)
    {
        var path = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.CrlPath));
        _crls = new Lazy<IReadOnlyList<X509Crl>>(() => Load(path, logger));
    }

    public X509Crl? FindByIssuer(X509Name issuerDn) =>
        _crls.Value.FirstOrDefault(crl => crl.IssuerDN.Equivalent(issuerDn));

    private static List<X509Crl> Load(string path, ILogger logger)
    {
        var result = new List<X509Crl>();

        if (!Directory.Exists(path))
        {
            logger.LogWarning("La carpeta de CRL locales no existe: {Path}.", path);
            return result;
        }

        var parser = new X509CrlParser();
        foreach (var file in Directory.EnumerateFiles(path, "*.crl"))
        {
            try
            {
                result.Add(parser.ReadCrl(File.ReadAllBytes(file)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo cargar la CRL {File}", file);
            }
        }

        logger.LogInformation("CRLs locales cargadas: {Count} desde {Path}.", result.Count, path);
        return result;
    }
}
