namespace ShopMicro.Domain;

/// <summary>
/// Dato ligero que <b>emite el agregado</b> cuando ocurre algo relevante. NO es el outbox:
/// el agregado solo declara lo que sabe (qué pasó, sobre qué agregado y con qué payload) y
/// se desentiende del resto. Los campos de infraestructura del outbox (id del evento, usuario,
/// timestamp) los rellena el handler, no el dominio: así el dominio no depende del Principal
/// ni del reloj. La conversión a la fila real <see cref="DomainEvent{TId}"/> ocurre en
/// infraestructura.
/// </summary>
/// <param name="EventType">tipo del evento, p. ej. "pizza:create"</param>
/// <param name="Aggregate">nombre del agregado origen, p. ej. "Pizza"</param>
/// <param name="IdAggregate">identidad del agregado origen</param>
/// <param name="Payload">carga del evento: un record, una clase o el propio agregado</param>
public sealed record DomainEventData(
    string EventType,
    string Aggregate,
    Guid IdAggregate,
    object Payload);
