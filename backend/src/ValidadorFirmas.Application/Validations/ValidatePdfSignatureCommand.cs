using MediatR;
using ValidadorFirmas.Application.Dtos;

namespace ValidadorFirmas.Application.Validations;

/// <summary>Comando: validar todas las firmas digitales contenidas en un PDF.</summary>
public sealed record ValidatePdfSignatureCommand(byte[] FileBytes, string FileName)
    : IRequest<DocumentValidationResponseDto>;
