using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace ValidadorFirmas.Infrastructure.Revocation;

/// <summary>
/// Valida que una URL de OCSP/CRL —extraída del certificado que se está validando, por lo
/// tanto potencialmente controlada por un atacante— sea segura para que el servidor la
/// consulte. Sin este guard, un certificado malicioso podría apuntar el responder OCSP o el
/// CRL Distribution Point a una dirección interna (SSRF): loopback, redes privadas, o
/// direcciones link-local como el endpoint de metadata de la nube (169.254.169.254).
/// </summary>
public static class SsrfGuard
{
    public static async Task<bool> IsUrlSafeAsync(string url, ILogger logger, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            LogBlocked(logger, url, "esquema no permitido");
            return false;
        }

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo resolver el host {Host} para verificar la URL {Url}.", uri.Host, url);
            return false;
        }

        if (addresses.Length == 0 || addresses.Any(a => !IsPublicAddress(a)))
        {
            LogBlocked(logger, url, "resuelve a una dirección no pública (loopback/privada/link-local)");
            return false;
        }

        return true;
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            if (b[0] == 0) return false; // 0.0.0.0/8
            if (b[0] == 10) return false; // 10.0.0.0/8
            if (b[0] == 172 && b[1] is >= 16 and <= 31) return false; // 172.16.0.0/12
            if (b[0] == 192 && b[1] == 168) return false; // 192.168.0.0/16
            if (b[0] == 169 && b[1] == 254) return false; // 169.254.0.0/16 (incluye metadata de nube)
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
                return false;

            var b = address.GetAddressBytes();
            if ((b[0] & 0xFE) == 0xFC) return false; // fc00::/7, unique local address

            return true;
        }

        return false;
    }

    private static void LogBlocked(ILogger logger, string url, string reason)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object> { ["LogCategory"] = "Security" });
        logger.LogWarning("Bloqueada consulta SSRF a {Url}: {Reason}.", url, reason);
    }
}
