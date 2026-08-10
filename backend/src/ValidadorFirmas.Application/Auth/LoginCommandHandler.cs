using MediatR;
using Microsoft.Extensions.Logging;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Application.Auth;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private const string GenericFailureMessage = "Email o contraseña incorrectos.";

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthTokenIssuer _tokenIssuer;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        AuthTokenIssuer tokenIssuer,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenIssuer = tokenIssuer;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Mismo mensaje y mismo camino de código tanto si el usuario no existe como si la
        // contraseña es incorrecta: evita que la respuesta permita enumerar emails registrados.
        if (user is null || !user.Activo || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            LogFailedAttempt(request.Email);
            throw new UnauthorizedException(GenericFailureMessage);
        }

        user.RegistrarAcceso();

        var authResult = await _tokenIssuer.IssueTokensAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return authResult;
    }

    private void LogFailedAttempt(string email)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["LogCategory"] = "Security" });
        _logger.LogWarning("Intento de login fallido para {Email}", email);
    }
}
