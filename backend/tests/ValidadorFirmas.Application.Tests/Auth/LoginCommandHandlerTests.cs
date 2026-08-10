using Microsoft.Extensions.Logging;
using NSubstitute;
using ValidadorFirmas.Application.Auth;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Shared.Exceptions;
using Xunit;

namespace ValidadorFirmas.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private LoginCommandHandler CreateHandler()
    {
        var tokenIssuer = new AuthTokenIssuer(_refreshTokenRepository, _jwtTokenService);
        return new LoginCommandHandler(
            _userRepository, _unitOfWork, _passwordHasher, tokenIssuer,
            Substitute.For<ILogger<LoginCommandHandler>>());
    }

    private static User CreateActiveUser(string email = "admin@meridional.com.py") =>
        new(email, "Admin", "hash", UserRole.Administrador);

    [Fact]
    public async Task Handle_ConCredencialesCorrectas_DevuelveTokens()
    {
        var user = CreateActiveUser();
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("correcta", user.PasswordHash).Returns(true);
        _jwtTokenService.CreateAccessToken(user).Returns(("access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
        _jwtTokenService.CreateRefreshToken().Returns(("raw-refresh", "hash-refresh", DateTimeOffset.UtcNow.AddDays(7)));

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginCommand(user.Email, "correcta"), CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh", result.RefreshToken);
        Assert.Equal(user.Email, result.Usuario.Email);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConContraseñaIncorrecta_LanzaUnauthorizedConMensajeGenerico()
    {
        var user = CreateActiveUser();
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("incorrecta", user.PasswordHash).Returns(false);

        var handler = CreateHandler();
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand(user.Email, "incorrecta"), CancellationToken.None));

        Assert.Equal("Email o contraseña incorrectos.", exception.Message);
    }

    [Fact]
    public async Task Handle_ConUsuarioInexistente_LanzaElMismoMensajeQueContraseñaIncorrecta()
    {
        _userRepository.GetByEmailAsync("nadie@meridional.com.py", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = CreateHandler();

        // El mensaje debe ser idéntico al de contraseña incorrecta: así no se puede usar la
        // respuesta del login para enumerar qué emails están registrados.
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("nadie@meridional.com.py", "cualquiera"), CancellationToken.None));

        Assert.Equal("Email o contraseña incorrectos.", exception.Message);
    }

    [Fact]
    public async Task Handle_ConUsuarioDesactivado_LanzaUnauthorized()
    {
        var user = CreateActiveUser();
        user.Desactivar();
        _userRepository.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand(user.Email, "correcta"), CancellationToken.None));
    }
}
