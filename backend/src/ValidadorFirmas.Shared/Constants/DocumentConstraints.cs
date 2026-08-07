namespace ValidadorFirmas.Shared.Constants;

/// <summary>
/// Restricciones aplicadas a los documentos subidos para validación.
/// </summary>
public static class DocumentConstraints
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB
    public const string AllowedContentType = "application/pdf";
    public const string AllowedExtension = ".pdf";

    /// <summary>Firma binaria ("%PDF-") que debe encabezar todo archivo PDF válido.</summary>
    public static readonly byte[] PdfMagicBytes = "%PDF-"u8.ToArray();
}
