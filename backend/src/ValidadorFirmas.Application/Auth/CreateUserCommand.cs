using FluentValidation;
using MediatR;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Application.Auth;

public sealed record CreateUserCommand(
    string Email,
    string NombreCompleto,
    string Password,
    UserRole Role) : IRequest<UserDto>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.NombreCompleto).NotEmpty();
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(10).WithMessage("La contraseña debe tener al menos 10 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe tener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
        RuleFor(c => c.Role).IsInEnum();
    }
}
