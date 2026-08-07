using System.Security.Cryptography.X509Certificates;
using ValidadorFirmas.Domain.ValueObjects;

namespace ValidadorFirmas.Application.Common;

/// <summary>
/// Extrae los datos del firmante desde el Subject (DN) y las extensiones de su certificado.
/// Es un mapeo best-effort: sin el perfil oficial de OIDs de cada CA paraguaya, no todos los
/// campos estarán siempre disponibles; se documenta como punto a refinar con certificados reales.
/// </summary>
public static class SignerInfoExtractor
{
    private const string OidCommonName = "2.5.4.3";
    private const string OidOrganization = "2.5.4.10";
    private const string OidOrganizationalUnit = "2.5.4.11";
    private const string OidSerialNumber = "2.5.4.5";

    public static SignerInfo Extract(X509Certificate2 certificate)
    {
        var rdnValues = new Dictionary<string, string>();
        foreach (var rdn in certificate.SubjectName.EnumerateRelativeDistinguishedNames())
        {
            var oid = rdn.GetSingleElementType().Value;
            var value = rdn.GetSingleElementValue();
            if (oid is not null && value is not null && !rdnValues.ContainsKey(oid))
                rdnValues[oid] = value;
        }

        var nombreCompleto = rdnValues.GetValueOrDefault(OidCommonName)
            ?? certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false)
            ?? "Desconocido";

        var correo = certificate.GetNameInfo(X509NameType.EmailName, forIssuer: false);

        return new SignerInfo(
            NombreCompleto: nombreCompleto,
            NumeroDocumento: rdnValues.GetValueOrDefault(OidSerialNumber),
            Correo: string.IsNullOrWhiteSpace(correo) ? null : correo,
            Empresa: rdnValues.GetValueOrDefault(OidOrganization),
            Cargo: rdnValues.GetValueOrDefault(OidOrganizationalUnit));
    }
}
