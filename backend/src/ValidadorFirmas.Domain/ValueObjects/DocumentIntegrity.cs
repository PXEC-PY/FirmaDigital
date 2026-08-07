namespace ValidadorFirmas.Domain.ValueObjects;

/// <summary>Resumen de integridad del documento a nivel de archivo (no de una firma individual).</summary>
public sealed record DocumentIntegrity(
    bool EsIntegro,
    int CantidadFirmas,
    string? Motivo);
