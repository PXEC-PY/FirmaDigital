namespace ValidadorFirmas.Application.Dtos;

public sealed record DocumentIntegrityDto(
    bool EsIntegro,
    int CantidadFirmas,
    string? Motivo);
