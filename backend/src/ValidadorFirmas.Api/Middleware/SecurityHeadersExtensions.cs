namespace ValidadorFirmas.Api.Middleware;

/// <summary>
/// Agrega los encabezados HTTP de seguridad recomendados por OWASP a toda respuesta: evitan
/// clickjacking (X-Frame-Options / CSP frame-ancestors), MIME sniffing, fuga de referrer, uso
/// no autorizado de APIs del navegador, y restringen de dónde puede cargar recursos la página.
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=(), payment=()";
            headers["Content-Security-Policy"] =
                "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

            await next();
        });
    }
}
