using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Api.Middleware;

/// <summary>
/// Traduce las excepciones de la aplicación a respuestas HTTP consistentes en formato
/// <see cref="ProblemDetails"/>, sin exponer detalles internos para errores no controlados.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, errors) = Map(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado procesando {Path}", httpContext.Request.Path);
        else
            _logger.LogInformation("{Title}: {Detail}", title, detail);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string Detail, IReadOnlyList<string>? Errors) Map(Exception exception) =>
        exception switch
        {
            AppValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                validationException.Message,
                validationException.Errors),

            DomainException domainException => (
                StatusCodes.Status422UnprocessableEntity,
                "No se pudo procesar el documento",
                domainException.Message,
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado al procesar la solicitud.",
                null)
        };
}
