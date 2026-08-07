namespace ValidadorFirmas.Domain.Enums;

/// <summary>Resultado de consultar el estado de revocación de un certificado (CRL y/o OCSP).</summary>
public enum RevocationStatus
{
    NoRevocado,
    Revocado,

    /// <summary>Ni CRL ni OCSP estuvieron disponibles para el certificado.</summary>
    NoVerificable
}
