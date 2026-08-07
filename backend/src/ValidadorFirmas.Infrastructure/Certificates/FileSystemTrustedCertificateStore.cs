using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Infrastructure.Options;

namespace ValidadorFirmas.Infrastructure.Certificates;

/// <summary>
/// Carga los certificados de confianza desde la carpeta configurada (TrustedCertificates/).
/// Los certificados autofirmados (Subject == Issuer) se consideran raíces; el resto,
/// intermedios. Si la carpeta está vacía o no existe, no lanza: registra una advertencia y
/// deja que el resto del sistema reporte "cadena no verificable" en vez de fallar.
/// </summary>
public sealed class FileSystemTrustedCertificateStore : ITrustedCertificateStore
{
    private static readonly string[] SupportedExtensions = [".cer", ".crt", ".pem", ".der"];

    private readonly Lazy<IReadOnlyList<X509Certificate2>> _roots;
    private readonly Lazy<IReadOnlyList<X509Certificate2>> _intermediates;

    public FileSystemTrustedCertificateStore(
        IOptions<TrustStoreOptions> options,
        IHostEnvironment environment,
        ILogger<FileSystemTrustedCertificateStore> logger)
    {
        var path = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RootCertificatesPath));

        var loaded = new Lazy<(List<X509Certificate2> Roots, List<X509Certificate2> Intermediates)>(() =>
            LoadCertificates(path, logger));

        _roots = new Lazy<IReadOnlyList<X509Certificate2>>(() => loaded.Value.Roots);
        _intermediates = new Lazy<IReadOnlyList<X509Certificate2>>(() => loaded.Value.Intermediates);
    }

    public IReadOnlyList<X509Certificate2> GetTrustedRoots() => _roots.Value;

    public IReadOnlyList<X509Certificate2> GetIntermediateCertificates() => _intermediates.Value;

    private static (List<X509Certificate2> Roots, List<X509Certificate2> Intermediates) LoadCertificates(
        string path, ILogger logger)
    {
        var roots = new List<X509Certificate2>();
        var intermediates = new List<X509Certificate2>();

        if (!Directory.Exists(path))
        {
            logger.LogWarning(
                "La carpeta de certificados de confianza no existe: {Path}. La cadena de confianza no podrá verificarse hasta agregar certificados raíz.",
                path);
            return (roots, intermediates);
        }

        var files = Directory.EnumerateFiles(path)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in files)
        {
            try
            {
                var certificate = X509CertificateLoader.LoadCertificateFromFile(file);
                if (certificate.SubjectName.RawData.AsSpan().SequenceEqual(certificate.IssuerName.RawData))
                    roots.Add(certificate);
                else
                    intermediates.Add(certificate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo cargar el certificado de confianza {File}", file);
            }
        }

        if (roots.Count == 0)
            logger.LogWarning("No se encontraron certificados raíz autofirmados en {Path}.", path);

        logger.LogInformation(
            "Almacén de confianza cargado: {RootCount} raíz(ces), {IntermediateCount} intermedio(s) desde {Path}.",
            roots.Count, intermediates.Count, path);

        return (roots, intermediates);
    }
}
