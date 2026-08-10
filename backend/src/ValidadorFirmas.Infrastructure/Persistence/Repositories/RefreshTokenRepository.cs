using Microsoft.EntityFrameworkCore;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Entities;

namespace ValidadorFirmas.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ValidadorFirmasDbContext _dbContext;

    public RefreshTokenRepository(ValidadorFirmasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
}
