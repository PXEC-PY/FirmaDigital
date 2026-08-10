using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Entities;

namespace ValidadorFirmas.Application.Auth;

internal static class UserMapper
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Email,
        user.NombreCompleto,
        user.Role.ToString(),
        user.Activo,
        user.CreatedAtUtc,
        user.UltimoAccesoUtc);
}
