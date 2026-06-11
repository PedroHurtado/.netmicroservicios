namespace ShopMicro.Catalog.Outbox;

using MediatR;
using ShopMicro.Domain;

/// <summary>
/// Envoltorio de infraestructura que hace publicable un <see cref="DomainEventData"/> por
/// MediatR. Aquí —y solo aquí— vive la dependencia de <see cref="INotification"/>: el dominio
/// (<see cref="DomainEventData"/> y <see cref="DomainEvent{TId}"/>) queda libre de MediatR.
/// </summary>
public sealed record DomainEventNotification(DomainEventData Data) : INotification;
