using MediatR;

namespace ValidadorFirmas.Application.Auth;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
