using Microsoft.Extensions.Logging.Abstractions;
using ValidadorFirmas.Infrastructure.Revocation;
using Xunit;

namespace ValidadorFirmas.Infrastructure.Tests.Revocation;

public class SsrfGuardTests
{
    [Theory]
    [InlineData("http://127.0.0.1/ocsp")]
    [InlineData("http://10.1.2.3/ocsp")]
    [InlineData("http://172.16.0.5/ocsp")]
    [InlineData("http://172.31.255.255/ocsp")]
    [InlineData("http://192.168.1.1/ocsp")]
    [InlineData("http://169.254.169.254/latest/meta-data")] // endpoint de metadata de nube
    [InlineData("http://0.0.0.0/ocsp")]
    public async Task IsUrlSafeAsync_ConDireccionNoPublica_DevuelveFalse(string url)
    {
        var isSafe = await SsrfGuard.IsUrlSafeAsync(url, NullLogger.Instance, CancellationToken.None);

        Assert.False(isSafe);
    }

    [Theory]
    [InlineData("ftp://8.8.8.8/ocsp")]
    [InlineData("file:///etc/passwd")]
    [InlineData("no-es-una-url")]
    public async Task IsUrlSafeAsync_ConEsquemaOFormatoInvalido_DevuelveFalse(string url)
    {
        var isSafe = await SsrfGuard.IsUrlSafeAsync(url, NullLogger.Instance, CancellationToken.None);

        Assert.False(isSafe);
    }

    [Theory]
    [InlineData("http://8.8.8.8/ocsp")]
    [InlineData("https://1.1.1.1/crl")]
    public async Task IsUrlSafeAsync_ConDireccionPublica_DevuelveTrue(string url)
    {
        var isSafe = await SsrfGuard.IsUrlSafeAsync(url, NullLogger.Instance, CancellationToken.None);

        Assert.True(isSafe);
    }
}
