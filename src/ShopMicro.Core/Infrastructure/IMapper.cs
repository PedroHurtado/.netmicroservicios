namespace ShopMicro.Infrastructure;

/// <summary>
/// Contrato común de los mappers dominio ↔ EF. Permite que las interfaces base
/// mapeen genéricamente sin conocer el tipo concreto de persistencia.
/// </summary>
/// <typeparam name="T">tipo de dominio</typeparam>
/// <typeparam name="TEf">tipo de persistencia (entidad EF)</typeparam>
public interface IMapper<T, TEf>
{
    TEf ToEf(T domain);

    T ToDomain(TEf ef);
}
