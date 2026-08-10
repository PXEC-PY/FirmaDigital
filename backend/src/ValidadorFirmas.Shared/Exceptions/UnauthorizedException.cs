namespace ValidadorFirmas.Shared.Exceptions;

/// <summary>Excepción lanzada cuando una credencial o token no es válido.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
