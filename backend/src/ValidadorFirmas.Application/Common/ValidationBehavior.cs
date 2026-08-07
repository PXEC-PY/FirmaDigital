using FluentValidation;
using MediatR;
using ValidadorFirmas.Shared.Exceptions;

namespace ValidadorFirmas.Application.Common;

/// <summary>Pipeline de MediatR que ejecuta los validadores de FluentValidation antes del handler.</summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (failures.Count > 0)
            throw new AppValidationException(failures);

        return await next(cancellationToken);
    }
}
