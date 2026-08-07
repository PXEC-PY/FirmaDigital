namespace ValidadorFirmas.Domain.Enums;

/// <summary>Mecanismo utilizado para determinar el estado de revocación de un certificado.</summary>
public enum RevocationSource
{
    Ninguna,
    Ocsp,
    Crl
}
