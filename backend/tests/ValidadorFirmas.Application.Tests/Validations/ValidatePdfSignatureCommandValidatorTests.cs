using ValidadorFirmas.Application.Validations;
using ValidadorFirmas.Shared.Constants;
using Xunit;

namespace ValidadorFirmas.Application.Tests.Validations;

public class ValidatePdfSignatureCommandValidatorTests
{
    private readonly ValidatePdfSignatureCommandValidator _validator = new();

    private static byte[] ValidPdfBytes() =>
        [.. DocumentConstraints.PdfMagicBytes, .. "1.7\n%%EOF"u8.ToArray()];

    [Fact]
    public void Validate_ConArchivoPdfValido_NoTieneErrores()
    {
        var command = new ValidatePdfSignatureCommand(ValidPdfBytes(), "documento.pdf");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ConArchivoVacio_TieneError()
    {
        var command = new ValidatePdfSignatureCommand([], "documento.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ConArchivoQueSuperaElTamañoMaximo_TieneError()
    {
        var oversized = new byte[DocumentConstraints.MaxFileSizeBytes + 1];
        DocumentConstraints.PdfMagicBytes.CopyTo(oversized, 0);
        var command = new ValidatePdfSignatureCommand(oversized, "documento.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_SinFirmaBinariaPdf_TieneError()
    {
        var command = new ValidatePdfSignatureCommand("no es un pdf"u8.ToArray(), "documento.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ConExtensionDistintaDePdf_TieneError()
    {
        var command = new ValidatePdfSignatureCommand(ValidPdfBytes(), "documento.docx");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
