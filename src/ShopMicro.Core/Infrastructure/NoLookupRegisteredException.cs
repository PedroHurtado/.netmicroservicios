namespace ShopMicro.Infrastructure;

/// <summary>
/// Se lanza al pedir al <see cref="LookupResolver"/> un tipo para el que no hay ningún
/// <see cref="ILookup{T}"/> registrado. Es un fallo de cableado (DI), no un dato no encontrado.
/// </summary>
public sealed class NoLookupRegisteredException : Exception
{
    public NoLookupRegisteredException(Type type)
        : base($"No hay Lookup registrado para el tipo {type.Name}.")
    {
    }
}
