namespace ValidadorFirmas.Domain.Enums;

/// <summary>Estado de vigencia de un certificado en el momento evaluado (firma o consulta).</summary>
public enum CertificateStatus
{
    Vigente,
    Revocado,
    Expirado,
    Desconocido
}
