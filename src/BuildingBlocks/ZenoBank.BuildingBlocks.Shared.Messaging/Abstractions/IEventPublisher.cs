

using ZenoBank.BuildingBlocks.Shared.Contracts.Events;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
