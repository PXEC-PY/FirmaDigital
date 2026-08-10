using Microsoft.Extensions.Logging;
using NSubstitute;
using ValidadorFirmas.Application.Auth;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Shared.Exceptions;
using Xunit;

namespace ValidadorFirmas.Application.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private RefreshTokenCommandHandler CreateHandler()
    {
        var tokenIssuer = new AuthTokenIssuer(_refreshTokenRepository, _jwtTokenService);
        return new RefreshTokenCommandHandler(
            _refreshTokenRepository, _userRepository, _unitOfWork, _jwtTokenService, tokenIssuer,
            Substitute.For<ILogger<RefreshTokenCommandHandler>>());
    }

    [Fact]
    public async Task Handle_ConTokenValido_RotaElTokenYDevuelveUnoNuevo()
    {
        var user = new User("auditor@meridional.com.py", "Auditor", "hash", UserRole.Auditor);
        var storedToken = new RefreshToken(user.Id, "hash-antiguo", DateTimeOffset.UtcNow.AddDays(1));

        _jwtTokenService.HashRefreshToken("raw-token").Returns("hash-antiguo");
        _refreshTokenRepository.GetByTokenHashAsync("hash-antiguo", Arg.Any<CancellationToken>()).Returns(storedToken);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenService.CreateAccessToken(user).Returns(("nuevo-access", DateTimeOffset.UtcNow.AddMinutes(15)));
        _jwtTokenService.CreateRefreshToken().Returns(("nuevo-raw", "nuevo-hash", DateTimeOffset.UtcNow.AddDays(7)));

        var handler = CreateHandler();
        var result = await handler.Handle(new RefreshTokenCommand("raw-token"), CancellationToken.None);

        Assert.Equal("nuevo-access", result.AccessToken);
        Assert.Equal("nuevo-raw", result.RefreshToken);
        Assert.False(storedToken.EstaActivo); // el token usado queda revocado (rotación)
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshToken>(t => t.TokenHash == "nuevo-hash"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConTokenInexistente_LanzaUnauthorized()
    {
        _jwtTokenService.HashRefreshToken(Arg.Any<string>()).Returns("hash-cualquiera");
        _refreshTokenRepository.GetByTokenHashAsync("hash-cualquiera", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("token-invalido"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ConTokenYaRevocado_LanzaUnauthorized()
    {
        var user = new User("auditor@meridional.com.py", "Auditor", "hash", UserRole.Auditor);
        var storedToken = new RefreshToken(user.Id, "hash-revocado", DateTimeOffset.UtcNow.AddDays(1));
        storedToken.Revocar();

        _jwtTokenService.HashRefreshToken(Arg.Any<string>()).Returns("hash-revocado");
        _refreshTokenRepository.GetByTokenHashAsync("hash-revocado", Arg.Any<CancellationToken>())
            .Returns(storedToken);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("token-reutilizado"), CancellationToken.None));
    }
}
