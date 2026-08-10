using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using Microsoft.Extensions.Logging.Abstractions;
using ValidadorFirmas.Infrastructure.Signatures;
using ValidadorFirmas.Infrastructure.Tests.TestUtils;
using ValidadorFirmas.Shared.Exceptions;
using Xunit;

namespace ValidadorFirmas.Infrastructure.Tests.Signatures;

public class PdfSignatureExtractorTests
{
    private readonly PdfSignatureExtractor _extractor = new(NullLogger<PdfSignatureExtractor>.Instance);

    [Fact]
    public void Extract_ConUnaFirma_DetectaIntegridadYAlgoritmosCorrectamente()
    {
        var (certificate, _, signature) = TestPdfSigner.CreateSelfSignedSigner("CN=Juan Perez, C=PY");
        var signedPdf = TestPdfSigner.Sign(TestPdfSigner.CreateBlankPdf(), "Signature1", certificate, signature);

        var result = _extractor.Extract(new MemoryStream(signedPdf));

        var extraction = Assert.Single(result);
        Assert.True(extraction.IntegridadCriptograficaValida);
        Assert.True(extraction.CubreDocumentoCompleto);
        Assert.Equal(1, extraction.NumeroRevision);
        Assert.Equal(1, extraction.TotalRevisiones);
        Assert.Equal("SHA256", extraction.AlgoritmoResumen);
        Assert.Equal("RSA", extraction.AlgoritmoFirma);
        Assert.Equal("Juan Perez", extraction.CertificadoFirmante.GetNameInfo(
            System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false));
        Assert.False(extraction.Timestamp.Presente);
    }

    [Fact]
    public void Extract_ConDosFirmas_SoloLaUltimaEsLaUltimaRevision()
    {
        var (cert1, _, sig1) = TestPdfSigner.CreateSelfSignedSigner("CN=Firmante Uno, C=PY");
        var (cert2, _, sig2) = TestPdfSigner.CreateSelfSignedSigner("CN=Firmante Dos, C=PY");

        var afterFirst = TestPdfSigner.Sign(TestPdfSigner.CreateBlankPdf(), "Signature1", cert1, sig1);
        var afterSecond = TestPdfSigner.Sign(afterFirst, "Signature2", cert2, sig2, appendMode: true);

        var result = _extractor.Extract(new MemoryStream(afterSecond));

        Assert.Equal(2, result.Count);
        var first = result.Single(r => r.NombreCampoFirma == "Signature1");
        var second = result.Single(r => r.NombreCampoFirma == "Signature2");

        Assert.Equal(1, first.NumeroRevision);
        Assert.Equal(2, first.TotalRevisiones);
        Assert.False(first.CubreDocumentoCompleto); // normal: se agregó una firma después
        Assert.True(first.IntegridadCriptograficaValida); // la firma en sí sigue siendo íntegra

        Assert.Equal(2, second.NumeroRevision);
        Assert.Equal(2, second.TotalRevisiones);
        Assert.True(second.CubreDocumentoCompleto);
        Assert.True(second.IntegridadCriptograficaValida);
    }

    [Fact]
    public void Extract_ConContenidoAlteradoDespuesDeFirmar_LaIntegridadEsInvalida()
    {
        const string marker = "TESTMARKER12345";

        using var stream = new MemoryStream();
        using (var writer = new PdfWriter(stream))
        using (var pdfDoc = new PdfDocument(writer))
        {
            pdfDoc.AddNewPage();
            // El diccionario Info nunca se comprime (a diferencia de los content streams),
            // así que el marcador queda garantizado como texto ASCII literal en el archivo.
            pdfDoc.GetDocumentInfo().SetMoreInfo("Marker", marker);
        }

        var (certificate, _, signature) = TestPdfSigner.CreateSelfSignedSigner("CN=Juan Perez, C=PY");
        var signedPdf = TestPdfSigner.Sign(stream.ToArray(), "Signature1", certificate, signature);

        var markerBytes = Encoding.ASCII.GetBytes(marker);
        var markerIndex = IndexOf(signedPdf, markerBytes);
        Assert.True(markerIndex >= 0, "No se encontró el marcador en el PDF firmado; no se puede alterar el contenido para la prueba.");

        var tampered = (byte[])signedPdf.Clone();
        tampered[markerIndex] = (byte)'X';

        var result = _extractor.Extract(new MemoryStream(tampered));

        var extraction = Assert.Single(result);
        Assert.False(extraction.IntegridadCriptograficaValida);
    }

    [Fact]
    public void Extract_ConPdfSinFirmas_LanzaDomainException()
    {
        var blankPdf = TestPdfSigner.CreateBlankPdf();

        Assert.Throws<DomainException>(() => _extractor.Extract(new MemoryStream(blankPdf)));
    }

    [Fact]
    public void Extract_ConOpenActionEmbebido_RechazaElDocumento()
    {
        using var stream = new MemoryStream();
        using (var writer = new PdfWriter(stream))
        using (var pdfDoc = new PdfDocument(writer))
        {
            pdfDoc.AddNewPage();
            pdfDoc.GetCatalog().SetOpenAction(PdfAction.CreateURI("https://ejemplo-malicioso.test"));
        }

        var (certificate, _, signature) = TestPdfSigner.CreateSelfSignedSigner("CN=Juan Perez, C=PY");
        var signedPdf = TestPdfSigner.Sign(stream.ToArray(), "Signature1", certificate, signature);

        Assert.Throws<DomainException>(() => _extractor.Extract(new MemoryStream(signedPdf)));
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var found = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { found = false; break; }
            }
            if (found) return i;
        }
        return -1;
    }
}
