namespace ValidadorFirmas.Shared.Exceptions;

/// <summary>
/// Excepción lanzada cuando la entrada a un caso de uso no cumple las reglas de validación.
/// Se nombra "AppValidationException" (en vez de "ValidationException") para no colisionar
/// con la excepción homónima de FluentValidation usada en la capa Application.
/// </summary>
public class AppValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public AppValidationException(IEnumerable<string> errors)
        : base("Se encontraron uno o más errores de validación.")
    {
        Errors = errors.ToList();
    }

    public AppValidationException(string error) : this([error])
    {
    }
}
