using MediatR;
using ValidadorFirmas.Application.Common.Ports;

namespace ValidadorFirmas.Application.Auth;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Idempotente: si el token no existe o ya estaba revocado, el logout igual "funciona".
        if (storedToken is { EstaActivo: true })
        {
            storedToken.Revocar();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
