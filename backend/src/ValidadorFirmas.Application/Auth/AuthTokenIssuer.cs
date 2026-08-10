using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Entities;

namespace ValidadorFirmas.Application.Auth;

/// <summary>Emite el par access token + refresh token para un usuario ya autenticado. Compartido entre login y refresh.</summary>
public sealed class AuthTokenIssuer
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthTokenIssuer(IRefreshTokenRepository refreshTokenRepository, IJwtTokenService jwtTokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, accessExpiresAt) = _jwtTokenService.CreateAccessToken(user);
        var (refreshToken, refreshTokenHash, refreshExpiresAt) = _jwtTokenService.CreateRefreshToken();

        await _refreshTokenRepository.AddAsync(
            new RefreshToken(user.Id, refreshTokenHash, refreshExpiresAt), cancellationToken);

        return new AuthResultDto(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt, user.ToDto());
    }
}
