using MediatR;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Application.Auth;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private const string GenericFailureMessage = "Sesión inválida o expirada. Iniciá sesión nuevamente.";

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthTokenIssuer _tokenIssuer;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        AuthTokenIssuer tokenIssuer,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _tokenIssuer = tokenIssuer;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.EstaActivo)
        {
            LogSuspiciousAttempt();
            throw new UnauthorizedException(GenericFailureMessage);
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null || !user.Activo)
        {
            LogSuspiciousAttempt();
            throw new UnauthorizedException(GenericFailureMessage);
        }

        // Rotación: el token usado queda revocado y se emite uno nuevo, aunque no haya expirado.
        storedToken.Revocar();

        var authResult = await _tokenIssuer.IssueTokensAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return authResult;
    }

    private void LogSuspiciousAttempt()
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["LogCategory"] = "Security" });
        _logger.LogWarning("Intento de refresh con un token inválido, expirado o ya revocado.");
    }
}
