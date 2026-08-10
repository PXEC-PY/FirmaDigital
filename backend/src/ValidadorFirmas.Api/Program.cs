using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Filters;
using ValidadorFirmas.Api.Middleware;
using ValidadorFirmas.Application;
using ValidadorFirmas.Application.Common.Ports;
using ValidadorFirmas.Infrastructure;
using ValidadorFirmas.Infrastructure.Options;
using ValidadorFirmas.Infrastructure.Persistence;
using ValidadorFirmas.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// Falla rápido si no hay una clave de firma JWT segura configurada — nunca arranca con una
// clave vacía o hardcodeada. Se lee de la variable de entorno Jwt__SigningKey.
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey) || jwtSigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "La variable de entorno Jwt__SigningKey no está configurada o tiene menos de 32 caracteres. " +
        "Generá una clave segura (por ejemplo: openssl rand -base64 48) antes de iniciar la aplicación.");
}

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("LogCategory", c => c == "Audit"))
        .WriteTo.File("logs/audit-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("LogCategory", c => c == "Security"))
        .WriteTo.File("logs/security-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90))
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty<string>("LogCategory", c => c is "Audit" or "Security"))
        .WriteTo.Console()
        .WriteTo.File("logs/application-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)));

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Validador de Firmas Digitales del Paraguay",
        Version = "v1",
        Description = "API de validación de firmas digitales en documentos PDF sobre la PKI del Paraguay."
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = DocumentConstraints.MaxFileSizeBytes;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = DocumentConstraints.MaxFileSizeBytes;
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Seguro por defecto: cualquier endpoint requiere estar autenticado salvo que tenga
// [AllowAnonymous] explícito (la validación de PDF y el login lo tienen).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fuerza bruta en login/refresh: pocos intentos por IP y por minuto.
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueLimit = 0;
    });

    // DoS básico sobre el endpoint público de validación.
    options.AddFixedWindowLimiter("validations", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 20;
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

using (var startupScope = app.Services.CreateScope())
{
    var dbContext = startupScope.ServiceProvider.GetRequiredService<ValidadorFirmasDbContext>();
    var passwordHasher = startupScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var startupLogger = startupScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbSeeder.MigrateAndSeedAsync(dbContext, passwordHasher, app.Configuration, startupLogger);
}

app.UseExceptionHandler();
app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Validador de Firmas Digitales v1"));
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Punto de entrada expuesto para las fábricas de <c>WebApplicationFactory</c> en tests.</summary>
public partial class Program;
