namespace ValidadorFirmas.Application.Dtos;

public sealed record ChainDto(
    string Estado,
    string? Motivo,
    IReadOnlyList<string> CadenaEmisores);
