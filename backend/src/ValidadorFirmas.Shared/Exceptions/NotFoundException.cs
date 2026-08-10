namespace ValidadorFirmas.Shared.Exceptions;

/// <summary>Excepción lanzada cuando un recurso solicitado por Id no existe.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
