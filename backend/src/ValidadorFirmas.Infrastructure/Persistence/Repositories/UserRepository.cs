using Microsoft.EntityFrameworkCore;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Domain.Entities;

namespace ValidadorFirmas.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ValidadorFirmasDbContext _dbContext;

    public UserRepository(ValidadorFirmasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Users.OrderBy(u => u.Email).ToListAsync(cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken) =>
        _dbContext.Users.AnyAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken) =>
        await _dbContext.Users.AddAsync(user, cancellationToken);
}
