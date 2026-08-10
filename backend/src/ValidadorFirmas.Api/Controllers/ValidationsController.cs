using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Application.Validations;
using ValidadorFirmas.Shared.Constants;

namespace ValidadorFirmas.Api.Controllers;

/// <summary>Validación de firmas digitales sobre documentos PDF. Función pública, no requiere sesión.</summary>
[ApiController]
[Route("api/v1/validations")]
[AllowAnonymous]
public sealed class ValidationsController : ControllerBase
{
    private readonly ISender _sender;

    public ValidationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Valida todas las firmas digitales contenidas en un documento PDF.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(DocumentConstraints.MaxFileSizeBytes)]
    [EnableRateLimiting("validations")]
    [ProducesResponseType(typeof(DocumentValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DocumentValidationResponseDto>> Validate(
        [FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Debe adjuntar un archivo PDF.");

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var command = new ValidatePdfSignatureCommand(stream.ToArray(), file.FileName);
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }
}
