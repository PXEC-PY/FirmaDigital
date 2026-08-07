using ValidadorFirmas.Domain.Entities;
using ValidadorFirmas.Domain.Enums;
using ValidadorFirmas.Domain.ValueObjects;
using Xunit;

namespace ValidadorFirmas.Application.Tests.Domain;

public class DocumentValidationResultTests
{
    private static SignatureValidation BuildSignature(
        CertificateStatus certificateStatus = CertificateStatus.Vigente,
        ChainStatus chainStatus = ChainStatus.Correcta,
        RevocationStatus revocationStatus = RevocationStatus.NoRevocado,
        bool integrityValid = true,
        bool coversWholeDocument = true,
        bool isLastRevision = true)
    {
        var revocation = new RevocationInfo(revocationStatus, RevocationSource.Crl, DateTimeOffset.UtcNow, null);
        var chain = new ChainValidationInfo(chainStatus, chainStatus == ChainStatus.Correcta ? null : "motivo", []);
        var certificate = new CertificateInfo(
            "CN=Emisor", "Emisor S.A.",
            DateTimeOffset.UtcNow.AddYears(-1), DateTimeOffset.UtcNow.AddYears(1),
            "01", "ABCD", certificateStatus, chain, revocation);

        return new SignatureValidation(
            nombreCampoFirma: "Signature1",
            firmante: new SignerInfo("Juan Pérez", "CI1234567", null, null, null),
            certificado: certificate,
            fechaFirma: DateTimeOffset.UtcNow,
            algoritmoResumen: "SHA256",
            algoritmoFirma: "RSA",
            integridadCriptograficaValida: integrityValid,
            cubreDocumentoCompleto: coversWholeDocument,
            esUltimaRevision: isLastRevision,
            timestamp: new TimestampInfo(false, null, null, null));
    }

    [Fact]
    public void Documento_ConFirmaValida_EsValido()
    {
        var signature = BuildSignature();
        var integrity = new DocumentIntegrity(true, 1, null);

        var result = new DocumentValidationResult("doc.pdf", "hash", [signature], integrity);

        Assert.Equal(OverallStatus.Valido, result.EstadoGeneral);
    }

    [Fact]
    public void Documento_ConCertificadoRevocado_EsInvalidoConMotivoEspecifico()
    {
        var signature = BuildSignature(certificateStatus: CertificateStatus.Revocado, revocationStatus: RevocationStatus.Revocado);
        var integrity = new DocumentIntegrity(true, 1, null);

        var result = new DocumentValidationResult("doc.pdf", "hash", [signature], integrity);

        Assert.Equal(OverallStatus.Invalido, result.EstadoGeneral);
        Assert.Equal("El certificado fue revocado.", result.Motivo);
    }

    [Fact]
    public void Documento_ConIntegridadCriptograficaInvalida_EsInvalido()
    {
        var signature = BuildSignature(integrityValid: false);
        var integrity = new DocumentIntegrity(true, 1, null);

        var result = new DocumentValidationResult("doc.pdf", "hash", [signature], integrity);

        Assert.Equal(OverallStatus.Invalido, result.EstadoGeneral);
        Assert.Equal("La firma es inválida.", result.Motivo);
    }

    [Fact]
    public void Documento_ConCadenaNoVerificable_EsAdvertencia()
    {
        var signature = BuildSignature(chainStatus: ChainStatus.NoVerificable);
        var integrity = new DocumentIntegrity(true, 1, null);

        var result = new DocumentValidationResult("doc.pdf", "hash", [signature], integrity);

        Assert.Equal(OverallStatus.Advertencia, result.EstadoGeneral);
    }

    [Fact]
    public void Documento_ModificadoDespuesDeLaUltimaFirma_EsInvalido()
    {
        var signature = BuildSignature(coversWholeDocument: false, isLastRevision: true);
        var integrity = new DocumentIntegrity(false, 1, "El documento fue modificado después de la última firma.");

        var result = new DocumentValidationResult("doc.pdf", "hash", [signature], integrity);

        Assert.Equal(OverallStatus.Invalido, result.EstadoGeneral);
    }

    [Fact]
    public void Documento_ConFirmaAnteriorQueNoCubreRevisionesPosteriores_NoEsInvalidaPorEsoSolo()
    {
        // Una firma que NO es la última revisión puede no cubrir el documento completo
        // (porque se agregaron firmas después); eso es normal, no una alteración.
        var signature = BuildSignature(coversWholeDocument: false, isLastRevision: false);

        Assert.Equal(OverallStatus.Valido, signature.Estado);
    }

    [Fact]
    public void Documento_SinFirmas_LanzaExcepcion()
    {
        var integrity = new DocumentIntegrity(true, 0, null);

        Assert.Throws<ValidadorFirmas.Shared.Exceptions.DomainException>(
            () => new DocumentValidationResult("doc.pdf", "hash", [], integrity));
    }
}
