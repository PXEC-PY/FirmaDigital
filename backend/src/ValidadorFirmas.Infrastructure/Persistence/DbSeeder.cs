using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones pendientes y, si no hay ningún usuario todavía, crea el Administrador
/// inicial a partir de <c>Admin__Email</c> / <c>Admin__InitialPassword</c> (variables de
/// entorno) — nunca hay un usuario con contraseña conocida hardcodeada en el código.
/// </summary>
public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(
        ValidadorFirmasDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
            return;

        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:InitialPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "No hay usuarios y no se configuraron Admin:Email / Admin:InitialPassword (variables de entorno " +
                "Admin__Email / Admin__InitialPassword): no se creó ningún usuario administrador inicial.");
            return;
        }

        var admin = new User(adminEmail, "Administrador", passwordHasher.Hash(adminPassword), UserRole.Administrador);
        await dbContext.Users.AddAsync(admin, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Se creó el usuario administrador inicial {Email}. Cambiá la contraseña apenas inicies sesión.",
            adminEmail);
    }
}
