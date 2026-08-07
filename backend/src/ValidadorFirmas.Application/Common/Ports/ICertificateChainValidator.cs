using System.Security.Cryptography.X509Certificates;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Application.Common.Ports;

/// <summary>Construye y evalúa la cadena de confianza de un certificado hasta una raíz confiable.</summary>
public interface ICertificateChainValidator
{
    /// <param name="leaf">Certificado del firmante.</param>
    /// <param name="intermediatesFromDocument">Certificados intermedios embebidos en la firma CMS.</param>
    ChainValidationInfo ValidateChain(
        X509Certificate2 leaf,
        IReadOnlyList<X509Certificate2> intermediatesFromDocument);

    /// <summary>
    /// Busca el certificado emisor de <paramref name="certificate"/> entre los certificados
    /// embebidos en el documento y los almacenes de confianza (raíces + intermedios), usando
    /// Authority/Subject Key Identifier cuando están disponibles y el nombre distinguido como
    /// respaldo. Devuelve null si no se encuentra.
    /// </summary>
    X509Certificate2? FindIssuer(X509Certificate2 certificate, IReadOnlyList<X509Certificate2> knownCertificates);
}
