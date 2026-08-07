using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi;
using Serilog;
using ValidadorFirmas.Api.Middleware;
using ValidadorFirmas.Application;
using ValidadorFirmas.Infrastructure;
using ValidadorFirmas.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

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

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Validador de Firmas Digitales v1"));
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
