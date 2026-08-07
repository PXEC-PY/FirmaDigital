namespace ValidadorFirmas.Domain.Enums;

/// <summary>Resultado de construir y validar la cadena de confianza de un certificado.</summary>
public enum ChainStatus
{
    Correcta,
    Incorrecta,

    /// <summary>No se pudo determinar porque no hay certificados raíz cargados en el almacén de confianza.</summary>
    NoVerificable
}
