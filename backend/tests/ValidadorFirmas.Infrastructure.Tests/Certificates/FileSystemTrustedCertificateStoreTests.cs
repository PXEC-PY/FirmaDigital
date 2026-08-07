using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ValidadorFirmas.Infrastructure.Certificates;
using ValidadorFirmas.Infrastructure.Options;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace ValidadorFirmas.Infrastructure.Tests.Certificates;

public class FileSystemTrustedCertificateStoreTests : IDisposable
{
    private readonly string _tempDirectory;

    public FileSystemTrustedCertificateStoreTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "vf-trust-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose() => Directory.Delete(_tempDirectory, recursive: true);

    [Fact]
    public void GetTrustedRoots_ClasificaAutofirmadosComoRaizYElRestoComoIntermedios()
    {
        using var rootRsa = RSA.Create(2048);
        var rootRequest = new CertificateRequest("CN=Raiz Prueba, C=PY", rootRsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        var rootCertificate = rootRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5));

        using var intermediateRsa = RSA.Create(2048);
        var intermediateRequest = new CertificateRequest("CN=Intermedia Prueba, C=PY", intermediateRsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        intermediateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        var serialNumber = new byte[] { 1, 2, 3, 4 };
        var intermediateCertificate = intermediateRequest.Create(
            rootCertificate, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(3), serialNumber);

        File.WriteAllBytes(Path.Combine(_tempDirectory, "root.crt"), rootCertificate.Export(X509ContentType.Cert));
        File.WriteAllBytes(Path.Combine(_tempDirectory, "intermediate.crt"), intermediateCertificate.Export(X509ContentType.Cert));

        var store = new FileSystemTrustedCertificateStore(
            MsOptions.Create(new TrustStoreOptions { RootCertificatesPath = _tempDirectory, CrlPath = _tempDirectory }),
            new FakeHostEnvironment(),
            NullLogger<FileSystemTrustedCertificateStore>.Instance);

        var roots = store.GetTrustedRoots();
        var intermediates = store.GetIntermediateCertificates();

        Assert.Single(roots);
        Assert.Equal("CN=Raiz Prueba, C=PY", roots[0].Subject);
        Assert.Single(intermediates);
        Assert.Equal("CN=Intermedia Prueba, C=PY", intermediates[0].Subject);
    }

    [Fact]
    public void GetTrustedRoots_ConCarpetaVacia_NoLanzaYDevuelveListaVacia()
    {
        var store = new FileSystemTrustedCertificateStore(
            MsOptions.Create(new TrustStoreOptions { RootCertificatesPath = _tempDirectory, CrlPath = _tempDirectory }),
            new FakeHostEnvironment(),
            NullLogger<FileSystemTrustedCertificateStore>.Instance);

        Assert.Empty(store.GetTrustedRoots());
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
