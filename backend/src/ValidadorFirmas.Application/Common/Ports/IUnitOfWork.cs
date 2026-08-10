namespace ValidadorFirmas.Application.Common.Ports;

/// <summary>Confirma en una sola transacción los cambios hechos a través de los repositorios.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
