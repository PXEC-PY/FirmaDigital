using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Infrastructure.Certificates;
using ValidadorFirmas.Infrastructure.Options;
using ValidadorFirmas.Infrastructure.Persistence;
using ValidadorFirmas.Infrastructure.Persistence.Repositories;
using ValidadorFirmas.Infrastructure.Revocation;
using ValidadorFirmas.Infrastructure.Security;
using ValidadorFirmas.Infrastructure.Signatures;

namespace ValidadorFirmas.Infrastructure;

/// <summary>Registro de dependencias de la capa Infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TrustStoreOptions>(configuration.GetSection(TrustStoreOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // AllowAutoRedirect = false: una URL de OCSP/CRL viene del certificado que se está
        // validando (potencialmente controlado por un atacante); sin esto, un 302 podría
        // sortear el guard SSRF que ya validó la URL original.
        services.AddHttpClient(nameof(RevocationChecker))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services.AddSingleton<ITrustedCertificateStore, FileSystemTrustedCertificateStore>();
        services.AddSingleton<LocalCrlStore>();

        services.AddScoped<IPdfSignatureExtractor, PdfSignatureExtractor>();
        services.AddScoped<ICertificateChainValidator, X509ChainValidator>();
        services.AddScoped<IRevocationChecker, RevocationChecker>();

        services.AddDbContext<ValidadorFirmasDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("Postgres");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "La cadena de conexión ConnectionStrings:Postgres no está configurada (variable de entorno " +
                    "ConnectionStrings__Postgres).");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
