namespace ValidadorFirmas.Shared.Exceptions;

/// <summary>
/// Excepción lanzada cuando se viola una regla o invariante del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
