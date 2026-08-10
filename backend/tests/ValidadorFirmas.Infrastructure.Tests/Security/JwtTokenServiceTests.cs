using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Infrastructure.Options;
using ValidadorFirmas.Infrastructure.Security;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace ValidadorFirmas.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private static JwtOptions ValidOptions() => new()
    {
        SigningKey = "una-clave-de-prueba-de-al-menos-32-caracteres-de-largo",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    private static User CreateUser() =>
        new("juan.perez@meridional.com.py", "Juan Pérez", "hash", UserRole.Auditor);

    [Fact]
    public void Constructor_ConClaveDeFirmaCorta_Lanza()
    {
        var options = ValidOptions();
        options.SigningKey = "muy-corta";

        Assert.Throws<InvalidOperationException>(() => new JwtTokenService(MsOptions.Create(options)));
    }

    [Fact]
    public void Constructor_SinClaveDeFirma_Lanza()
    {
        var options = ValidOptions();
        options.SigningKey = "";

        Assert.Throws<InvalidOperationException>(() => new JwtTokenService(MsOptions.Create(options)));
    }

    [Fact]
    public void CreateAccessToken_IncluyeLosClaimsDelUsuario()
    {
        var service = new JwtTokenService(MsOptions.Create(ValidOptions()));
        var user = CreateUser();

        var (token, expiresAt) = service.CreateAccessToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.Role.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.True(expiresAt > DateTimeOffset.UtcNow.AddMinutes(14));
        Assert.True(expiresAt <= DateTimeOffset.UtcNow.AddMinutes(15));
    }

    [Fact]
    public void CreateRefreshToken_ElHashCoincideConHashRefreshTokenDelValorCrudo()
    {
        var service = new JwtTokenService(MsOptions.Create(ValidOptions()));

        var (rawToken, tokenHash, _) = service.CreateRefreshToken();

        Assert.Equal(tokenHash, service.HashRefreshToken(rawToken));
    }

    [Fact]
    public void CreateRefreshToken_GeneraValoresDistintosEnCadaLlamada()
    {
        var service = new JwtTokenService(MsOptions.Create(ValidOptions()));

        var (raw1, _, _) = service.CreateRefreshToken();
        var (raw2, _, _) = service.CreateRefreshToken();

        Assert.NotEqual(raw1, raw2);
    }
}
