using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Domain.Entities;

/// <summary>Cuenta con acceso a la zona administrativa (no se requiere para validar documentos).</summary>
public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool Activo { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UltimoAccesoUtc { get; private set; }

    private User()
    {
    }

    public User(string email, string nombreCompleto, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email del usuario es requerido.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El usuario debe tener una contraseña.");

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        NombreCompleto = nombreCompleto;
        PasswordHash = passwordHash;
        Role = role;
        Activo = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RegistrarAcceso() => UltimoAccesoUtc = DateTimeOffset.UtcNow;

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;

    public void CambiarPasswordHash(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash))
            throw new DomainException("El hash de la contraseña no puede estar vacío.");
        PasswordHash = nuevoHash;
    }
}
