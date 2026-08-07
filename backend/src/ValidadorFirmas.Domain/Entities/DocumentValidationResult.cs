using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Domain.Entities;

/// <summary>
/// Raíz de agregado: resultado completo de validar un documento PDF, incluyendo todas sus
/// firmas y el veredicto general que se muestra al usuario.
/// </summary>
public sealed class DocumentValidationResult
{
    public Guid Id { get; }
    public string NombreArchivo { get; }
    public string HashSha256 { get; }
    public DateTimeOffset FechaValidacionUtc { get; }
    public IReadOnlyList<SignatureValidation> Firmas { get; }
    public DocumentIntegrity Integridad { get; }
    public OverallStatus EstadoGeneral { get; }
    public string Motivo { get; }

    public DocumentValidationResult(
        string nombreArchivo,
        string hashSha256,
        IReadOnlyList<SignatureValidation> firmas,
        DocumentIntegrity integridad)
    {
        if (firmas.Count == 0)
            throw new DomainException("El documento no contiene firmas digitales.");

        Id = Guid.NewGuid();
        NombreArchivo = nombreArchivo;
        HashSha256 = hashSha256;
        FechaValidacionUtc = DateTimeOffset.UtcNow;
        Firmas = firmas;
        Integridad = integridad;

        (EstadoGeneral, Motivo) = Evaluar();
    }

    private (OverallStatus, string) Evaluar()
    {
        if (!Integridad.EsIntegro)
            return (OverallStatus.Invalido, Integridad.Motivo ?? "El documento fue modificado.");

        var firmaInvalida = Firmas.FirstOrDefault(f => f.Estado == OverallStatus.Invalido);
        if (firmaInvalida is not null)
            return (OverallStatus.Invalido, firmaInvalida.Motivo ?? "La firma es inválida.");

        var firmaConAdvertencia = Firmas.FirstOrDefault(f => f.Estado == OverallStatus.Advertencia);
        if (firmaConAdvertencia is not null)
            return (OverallStatus.Advertencia, firmaConAdvertencia.Motivo ?? "El documento presenta advertencias.");

        return (OverallStatus.Valido, "El documento es íntegro y todas sus firmas son válidas.");
    }
}
