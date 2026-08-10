using MediatR;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Application.Auth;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new AppValidationException("Ya existe un usuario con ese email.");

        var user = new User(
            request.Email, request.NombreCompleto, _passwordHasher.Hash(request.Password), request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
