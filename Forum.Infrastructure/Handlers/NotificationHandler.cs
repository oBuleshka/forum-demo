using Forum.Domain.Events;
using Forum.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Forum.Infrastructure.Handlers;

public class NotificationHandler
{
    private readonly IHubContext<PostHub> _hubContext;

    public NotificationHandler(IHubContext<PostHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.Payload is not PostDomainEventPayload payload)
        {
            return;
        }

        await _hubContext.Clients.Group(PostHub.GetGroupName(payload.PostId)).SendAsync(
            "PostUpdated",
            new
            {
                domainEvent.Type,
                payload.PostId,
                payload.UserId,
                payload.EventType,
                payload.Message,
                domainEvent.CreatedAt
            },
            cancellationToken);
    }
}
