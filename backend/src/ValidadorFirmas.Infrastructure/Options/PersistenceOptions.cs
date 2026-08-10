namespace ValidadorFirmas.Infrastructure.Options;

/// <summary>Ubicación de la base de datos SQLite (usuarios, refresh tokens).</summary>
public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>Carpeta donde vive el archivo .db, relativa al content root o absoluta.</summary>
    public string DatabaseDirectory { get; set; } = "../../../Data";

    public string DatabaseFileName { get; set; } = "validador.db";
}
