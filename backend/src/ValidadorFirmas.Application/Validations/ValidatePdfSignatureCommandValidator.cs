using FluentValidation;
using ValidadorFirmas.Shared.Constants;

namespace ValidadorFirmas.Application.Validations;

public sealed class ValidatePdfSignatureCommandValidator : AbstractValidator<ValidatePdfSignatureCommand>
{
    public ValidatePdfSignatureCommandValidator()
    {
        RuleFor(c => c.FileBytes)
            .NotEmpty().WithMessage("El archivo está vacío.")
            .Must(bytes => bytes.Length <= DocumentConstraints.MaxFileSizeBytes)
            .WithMessage($"El archivo supera el tamaño máximo permitido de {DocumentConstraints.MaxFileSizeBytes / (1024 * 1024)} MB.")
            .Must(StartsWithPdfMagicBytes)
            .WithMessage("El archivo no es un PDF válido.");

        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage("El nombre del archivo es requerido.")
            .Must(name => name.EndsWith(DocumentConstraints.AllowedExtension, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Solo se admiten archivos PDF.");
    }

    private static bool StartsWithPdfMagicBytes(byte[] bytes)
    {
        var magic = DocumentConstraints.PdfMagicBytes;
        return bytes.Length >= magic.Length && bytes.AsSpan(0, magic.Length).SequenceEqual(magic);
    }
}
