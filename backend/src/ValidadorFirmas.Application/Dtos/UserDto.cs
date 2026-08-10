namespace ValidadorFirmas.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string NombreCompleto,
    string Role,
    bool Activo,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UltimoAccesoUtc);
