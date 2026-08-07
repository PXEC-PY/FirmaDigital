namespace ValidadorFirmas.Application.Dtos;

public sealed record SignerDto(
    string NombreCompleto,
    string? NumeroDocumento,
    string? Correo,
    string? Empresa,
    string? Cargo);
