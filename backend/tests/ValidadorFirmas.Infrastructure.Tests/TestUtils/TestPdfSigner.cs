using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;

namespace ValidadorFirmas.Infrastructure.Tests.TestUtils;

/// <summary>
/// Genera PDFs firmados con certificados autofirmados, únicamente para pruebas: permite
/// verificar el pipeline de extracción/validación de firmas sin depender de un PDF externo.
/// </summary>
internal static class TestPdfSigner
{
    public static byte[] CreateBlankPdf()
    {
        using var stream = new MemoryStream();
        using (var writer = new PdfWriter(stream))
        using (var pdfDoc = new PdfDocument(writer))
        {
            pdfDoc.AddNewPage();
        }
        return stream.ToArray();
    }

    public static (IX509Certificate ITextCertificate, X509Certificate2 DotNetCertificate, PrivateKeySignature Signature)
        CreateSelfSignedSigner(string subjectName, int validDays = 365)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(validDays));

        var bcCertificate = DotNetUtilities.FromX509Certificate(certificate);
        var bcKeyPair = DotNetUtilities.GetRsaKeyPair(rsa);

        IX509Certificate iTextCertificate = new X509CertificateBC(bcCertificate);
        var iTextPrivateKey = new PrivateKeyBC(bcKeyPair.Private);
        var signature = new PrivateKeySignature(iTextPrivateKey, "SHA256");

        return (iTextCertificate, certificate, signature);
    }

    public static byte[] Sign(byte[] pdfBytes, string fieldName, IX509Certificate certificate, PrivateKeySignature signature, bool appendMode = false)
    {
        using var output = new MemoryStream();
        using (var reader = new PdfReader(new MemoryStream(pdfBytes)))
        {
            var stampingProperties = appendMode ? new StampingProperties().UseAppendMode() : new StampingProperties();
            var signer = new PdfSigner(reader, output, stampingProperties);
            signer.SetSignerProperties(new SignerProperties().SetFieldName(fieldName));
            signer.SignDetached(signature, [certificate], null, null, null, 0, PdfSigner.CryptoStandard.CMS);
        }
        return output.ToArray();
    }
}
