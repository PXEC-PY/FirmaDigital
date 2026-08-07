namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>
/// Datos del firmante, extraídos del Subject (DN) y de las extensiones del certificado.
/// La extracción es best-effort: sin el perfil oficial de OIDs de cada CA paraguaya,
/// no todos los campos estarán siempre disponibles.
/// </summary>
public sealed record SignerInfo(
    string NombreCompleto,
    string? NumeroDocumento,
    string? Correo,
    string? Empresa,
    string? Cargo);
