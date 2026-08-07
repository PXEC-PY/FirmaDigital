using System.Security.Cryptography.X509Certificates;

namespace ValidadorFirmas.Application.Common.Ports;

/// <summary>
/// Da acceso a los certificados de confianza configurados (carpeta TrustedCertificates/).
/// Permite agregar nuevas Autoridades Certificadoras (o de otros países) sin tocar código:
/// alcanza con agregar el archivo de certificado a la carpeta.
/// </summary>
public interface ITrustedCertificateStore
{
    /// <summary>Certificados raíz (autofirmados) que actúan como ancla de confianza.</summary>
    IReadOnlyList<X509Certificate2> GetTrustedRoots();

    /// <summary>Certificados intermedios/subordinados conocidos, para completar cadenas incompletas.</summary>
    IReadOnlyList<X509Certificate2> GetIntermediateCertificates();
}
