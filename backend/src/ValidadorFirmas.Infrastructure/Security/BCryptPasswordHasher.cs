using ValidadorFirmas.Application.Common.Ports;

namespace ValidadorFirmas.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con BCrypt (work factor 12). Se eligió sobre Argon2 por ser una
/// implementación 100% managed, sin dependencias nativas que puedan fallar en un hosting
/// compartido — el checklist de seguridad acepta explícitamente "Argon2 o BCrypt".
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
