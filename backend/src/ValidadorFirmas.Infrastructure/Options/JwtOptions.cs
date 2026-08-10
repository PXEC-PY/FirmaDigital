namespace ValidadorFirmas.Infrastructure.Options;

/// <summary>
/// Configuración de emisión de JWT. La clave de firma se lee siempre de una variable de entorno
/// (<c>Jwt__SigningKey</c> o <c>JWT_SIGNING_KEY</c>), nunca se hardcodea ni se versiona.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ValidadorFirmasParaguay";
    public string Audience { get; set; } = "ValidadorFirmasParaguay.Clientes";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
