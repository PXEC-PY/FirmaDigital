using ValidadorFirmas.Application.Common.Ports;

namespace ValidadorFirmas.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ValidadorFirmasDbContext _dbContext;

    public UnitOfWork(ValidadorFirmasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await _dbContext.SaveChangesAsync(cancellationToken);
}
