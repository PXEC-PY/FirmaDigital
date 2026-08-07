using System.Security.Cryptography.X509Certificates;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Application.Common.Ports;

/// <summary>
/// Consulta el estado de revocación de un certificado, prefiriendo OCSP (si el certificado
/// publica un responder) y usando CRL —local o remota— como respaldo.
/// </summary>
public interface IRevocationChecker
{
    /// <param name="referenceTime">
    /// Momento respecto del cual se evalúa la revocación (idealmente la fecha de firma/sellado
    /// de tiempo, no "ahora"): una firma sigue siendo válida si el certificado fue revocado
    /// después de haber firmado.
    /// </param>
    Task<RevocationInfo> CheckRevocationAsync(
        X509Certificate2 certificate,
        X509Certificate2? issuer,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken);
}
