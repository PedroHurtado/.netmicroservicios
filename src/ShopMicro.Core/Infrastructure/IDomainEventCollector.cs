namespace ShopMicro.Infrastructure;

using ShopMicro.Domain;

/// <summary>
/// Puente repositorio → unidad de trabajo. En este proyecto el <c>DbContext</c> solo trackea
/// entidades EF (la fila), nunca el dominio: por tanto el agregado —que es quien acumula los
/// eventos— jamás entra en el <c>ChangeTracker</c> y un barrido por él no encontraría nada.
/// El único punto donde aún tenemos el agregado de dominio en la mano es el <c>AddAsync</c>
/// genérico; desde ahí lo "entregamos" a la unidad de trabajo con <see cref="Collect"/> para
/// que el despacho de <c>SaveChanges</c> publique sus eventos antes del commit.
/// </summary>
public interface IDomainEventCollector
{
    void Collect(IHasDomainEvents aggregate);
}
