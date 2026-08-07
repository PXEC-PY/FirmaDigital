using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Infrastructure.Certificates;
using ValidadorFirmas.Infrastructure.Options;
using ValidadorFirmas.Infrastructure.Revocation;
using ValidadorFirmas.Infrastructure.Signatures;

namespace ValidadorFirmas.Infrastructure;

/// <summary>Registro de dependencias de la capa Infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TrustStoreOptions>(configuration.GetSection(TrustStoreOptions.SectionName));

        services.AddHttpClient(nameof(RevocationChecker));

        services.AddSingleton<ITrustedCertificateStore, FileSystemTrustedCertificateStore>();
        services.AddSingleton<LocalCrlStore>();

        services.AddScoped<IPdfSignatureExtractor, PdfSignatureExtractor>();
        services.AddScoped<ICertificateChainValidator, X509ChainValidator>();
        services.AddScoped<IRevocationChecker, RevocationChecker>();

        return services;
    }
}
