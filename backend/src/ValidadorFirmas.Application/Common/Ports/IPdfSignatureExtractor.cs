using ValidadorFirmas.Application.Common.Models;

namespace ValidadorFirmas.Application.Common.Ports;

/// <summary>Extrae y verifica criptográficamente cada firma presente en un documento PDF.</summary>
public interface IPdfSignatureExtractor
{
    /// <summary>
    /// Lee el PDF y devuelve la extracción cruda de cada firma encontrada.
    /// Lanza <see cref="Shared.Exceptions.DomainException"/> si el documento no puede leerse
    /// (no es un PDF válido o está corrupto).
    /// </summary>
    IReadOnlyList<RawSignatureExtraction> Extract(Stream pdfStream);
}
