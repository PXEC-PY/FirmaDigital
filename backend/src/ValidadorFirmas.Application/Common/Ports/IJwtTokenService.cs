using ValidadorFirmas.Domain.Entities;

namespace ValidadorFirmas.Application.Common.Ports;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(User user);

    /// <summary>Genera un refresh token opaco. Devuelve el valor crudo (para el cliente) y su hash (para persistir).</summary>
    (string RawToken, string TokenHash, DateTimeOffset ExpiresAtUtc) CreateRefreshToken();

    /// <summary>Calcula el mismo hash que <see cref="CreateRefreshToken"/>, para buscar un token recibido del cliente.</summary>
    string HashRefreshToken(string rawToken);
}
