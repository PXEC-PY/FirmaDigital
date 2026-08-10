using ValidadorFirmas.Infrastructure.Security;
using Xunit;

namespace ValidadorFirmas.Infrastructure.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_NuncaDevuelveLaContraseñaEnClaro()
    {
        var hash = _hasher.Hash("MiContraseñaSegura123");

        Assert.DoesNotContain("MiContraseñaSegura123", hash);
    }

    [Fact]
    public void Verify_ConLaContraseñaCorrecta_DevuelveTrue()
    {
        var hash = _hasher.Hash("MiContraseñaSegura123");

        Assert.True(_hasher.Verify("MiContraseñaSegura123", hash));
    }

    [Fact]
    public void Verify_ConLaContraseñaIncorrecta_DevuelveFalse()
    {
        var hash = _hasher.Hash("MiContraseñaSegura123");

        Assert.False(_hasher.Verify("OtraContraseña", hash));
    }

    [Fact]
    public void Verify_ConUnHashMalformado_DevuelveFalseEnVezDeLanzar()
    {
        Assert.False(_hasher.Verify("cualquiera", "esto-no-es-un-hash-bcrypt"));
    }

    [Fact]
    public void Hash_ConLaMismaContraseñaDosVeces_ProduceHashesDistintos()
    {
        // BCrypt usa una sal aleatoria por llamada: dos hashes de la misma contraseña no deben
        // coincidir nunca (si coincidieran, indicaría que no se está salando).
        var hash1 = _hasher.Hash("MiContraseñaSegura123");
        var hash2 = _hasher.Hash("MiContraseñaSegura123");

        Assert.NotEqual(hash1, hash2);
    }
}
