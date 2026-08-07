namespace ValidadorFirmas.Application.Dtos;

/// <summary>Forma de respuesta de <c>POST /api/v1/validations</c>. Ver sección "DATOS A MOSTRAR".</summary>
public sealed record DocumentValidationResponseDto(
    Guid DocumentoId,
    string NombreArchivo,
    string HashSha256,
    DateTimeOffset FechaValidacion,
    string EstadoGeneral,
    string Motivo,
    DocumentIntegrityDto Documento,
    IReadOnlyList<SignatureDto> Firmas);
