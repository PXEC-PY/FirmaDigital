namespace ValidadorFirmas.Shared.Results;

/// <summary>
/// Representa el resultado de una operación que puede fallar sin recurrir a excepciones
/// para el flujo de control normal.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Un resultado exitoso no puede tener un mensaje de error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Un resultado fallido debe tener un mensaje de error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Variante de <see cref="Result"/> que además transporta un valor cuando la operación tiene éxito.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    protected internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}
