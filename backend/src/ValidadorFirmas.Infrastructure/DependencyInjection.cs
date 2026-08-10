using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddHttpClient(nameof(RevocationChecker));

        services.AddSingleton<ITrustedCertificateStore, FileSystemTrustedCertificateStore>();
        services.AddSingleton<LocalCrlStore>();

        services.AddScoped<IPdfSignatureExtractor, PdfSignatureExtractor>();
        services.AddScoped<ICertificateChainValidator, X509ChainValidator>();
        services.AddScoped<IRevocationChecker, RevocationChecker>();

        services.AddDbContext<ValidadorFirmasDbContext>((serviceProvider, options) =>
        {
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var persistenceOptions = serviceProvider.GetRequiredService<IOptions<PersistenceOptions>>().Value;

            var directory = Path.GetFullPath(
                Path.Combine(environment.ContentRootPath, persistenceOptions.DatabaseDirectory));
            Directory.CreateDirectory(directory);

            var databasePath = Path.Combine(directory, persistenceOptions.DatabaseFileName);
            options.UseSqlite($"Data Source={databasePath}");
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
