using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ValidadorFirmas.Application.Auth;
using ValidadorFirmas.Application.Common;

namespace ValidadorFirmas.Application;

/// <summary>Registro de dependencias de la capa Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<AuthTokenIssuer>();

        return services;
    }
}
